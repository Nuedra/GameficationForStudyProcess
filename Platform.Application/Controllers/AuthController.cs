using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform.Application.Authentication;
using Platform.Application.Contracts;
using Platform.Application.Models;
using Platform.Application.Services;
using Platform.Core.Models;

namespace Platform.Application.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public sealed class AuthController(
    IUserIdentityService userIdentityService,
    TimeProvider timeProvider,
    ILogger<AuthController> logger) : ControllerBase
{
    [HttpGet("csrf")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CsrfTokenDto), StatusCodes.Status200OK)]
    public ActionResult<CsrfTokenDto> GetCsrfToken(
        [FromServices] IAntiforgery antiforgery)
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);
        return Ok(new CsrfTokenDto(tokens.RequestToken!));
    }

    /// <summary>
    /// Выполняет вход пользователя по GUID.
    /// </summary>
    /// <remarks>
    /// Роль определяется только сервером: студент ищется в БД, а преподаватель и
    /// администратор — в конфигурации приложения.
    /// </remarks>
    /// <param name="request">GUID пользователя для входа.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    /// <returns>Данные вошедшего пользователя и назначенная сервером роль.</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthenticatedUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthenticatedUserDto>> Login(
        GuidLoginRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Id == Guid.Empty)
        {
            logger.LogWarning(
                "GUID login rejected: empty identifier. RemoteIp={RemoteIp}",
                HttpContext.Connection.RemoteIpAddress);
            return BadRequest(ApiErrors.InvalidUserId);
        }

        var user = await userIdentityService.ResolveByIdAsync(request.Id, cancellationToken);
        if (user is null)
        {
            logger.LogWarning(
                "GUID login rejected. UserIdPrefix={UserIdPrefix}, RemoteIp={RemoteIp}",
                request.Id.ToString("N")[..8],
                HttpContext.Connection.RemoteIpAddress);
            return Unauthorized(ApiErrors.InvalidCredentials);
        }

        var sessionId = Guid.NewGuid();
        var issuedUtc = timeProvider.GetUtcNow();
        var expiresUtc = issuedUtc.AddHours(8);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.DisplayName),
            new Claim(ClaimTypes.Role, UserRoleDictionary.Values[user.Role]),
            new Claim(PlatformClaimTypes.SessionId, sessionId.ToString())
        }.ToList();
        if (!string.IsNullOrWhiteSpace(user.Group))
            claims.Add(new Claim(PlatformClaimTypes.Group, user.Group));
        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = false,
                IssuedUtc = issuedUtc,
                ExpiresUtc = expiresUtc
            });

        logger.LogInformation(
            "GUID login succeeded. UserId={UserId}, Role={Role}, SessionId={SessionId}, RemoteIp={RemoteIp}",
            user.Id,
            UserRoleDictionary.Values[user.Role],
            sessionId,
            HttpContext.Connection.RemoteIpAddress);

        return Ok(ToDto(user));
    }

    /// <summary>
    /// Возвращает текущего авторизованного пользователя.
    /// </summary>
    /// <remarks>
    /// Endpoint нужен фронту, чтобы после перезагрузки страницы понять, есть ли активная
    /// сессия и кому она принадлежит.
    /// </remarks>
    /// <returns>Данные текущего пользователя.</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(AuthenticatedUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status401Unauthorized)]
    public ActionResult<AuthenticatedUserDto> GetCurrentUser()
    {
        if (!TryBuildCurrentUser(out var user))
            return Unauthorized(ApiErrors.AuthenticationRequired);

        return Ok(user);
    }

    [HttpGet("session")]
    [Authorize]
    [ProducesResponseType(typeof(AuthenticatedSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthenticatedSessionDto>> GetCurrentSession()
    {
        if (!TryBuildCurrentUser(out var user) ||
            !Guid.TryParse(User.FindFirstValue(PlatformClaimTypes.SessionId), out var sessionId))
        {
            return Unauthorized(ApiErrors.AuthenticationRequired);
        }

        var authentication = await HttpContext.AuthenticateAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok(new AuthenticatedSessionDto(
            user,
            sessionId,
            authentication.Properties?.IssuedUtc,
            authentication.Properties?.ExpiresUtc));
    }

    /// <summary>
    /// Завершает текущую пользовательскую сессию.
    /// </summary>
    /// <remarks>
    /// Удаляет authentication cookie. После этого защищённые endpoints снова будут
    /// возвращать `401 Unauthorized`.
    /// </remarks>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue(ClaimTypes.Role);
        var sessionId = User.FindFirstValue(PlatformClaimTypes.SessionId);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        logger.LogInformation(
            "Authentication session ended. UserId={UserId}, Role={Role}, SessionId={SessionId}, RemoteIp={RemoteIp}",
            userId,
            role,
            sessionId,
            HttpContext.Connection.RemoteIpAddress);
        return NoContent();
    }

    private bool TryBuildCurrentUser(out AuthenticatedUserDto user)
    {
        if (!User.TryGetUserId(out var userId) ||
            !User.TryGetUserRole(out var role))
        {
            user = null!;
            return false;
        }

        user = new AuthenticatedUserDto(
            userId,
            User.Identity?.Name ?? string.Empty,
            role,
            User.FindFirstValue(PlatformClaimTypes.Group));
        return true;
    }

    private static AuthenticatedUserDto ToDto(ResolvedUserIdentity user)
    {
        return new AuthenticatedUserDto(
            user.Id,
            user.DisplayName,
            user.Role,
            user.Group);
    }
}
