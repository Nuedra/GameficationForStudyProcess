using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform.Application.Authentication;
using Platform.Application.Contracts;
using Platform.Application.Services;
using Platform.Core.Models;

namespace Platform.Application.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public sealed class AuthController(IStudentIdentityService studentIdentityService) : ControllerBase
{
    /// <summary>
    /// Выполняет вход студента по ID.
    /// </summary>
    /// <remarks>
    /// Если студент с указанным ID найден, сервер создаёт cookie `Platform.Student`.
    /// Эту cookie фронт использует в следующих запросах к студенческим endpoints.
    /// </remarks>
    /// <param name="request">ID студента для входа.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    /// <returns>Данные вошедшего студента.</returns>
    [HttpPost("student/login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(StudentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<StudentDto>> Login(
        StudentLoginRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Id == Guid.Empty)
            return BadRequest(ApiErrors.InvalidStudentId);

        var student = await studentIdentityService.FindByIdAsync(request.Id, cancellationToken);
        if (student is null)
            return Unauthorized(ApiErrors.InvalidCredentials);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, student.Id.ToString()),
            new Claim(ClaimTypes.Name, student.FullName),
            new Claim(ClaimTypes.Role, UserRoleDictionary.Values[UserRole.Student])
        };
        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true
            });

        return Ok(student);
    }

    /// <summary>
    /// Возвращает текущего авторизованного студента.
    /// </summary>
    /// <remarks>
    /// Endpoint нужен фронту, чтобы после перезагрузки страницы понять, есть ли активная
    /// студенческая сессия и кому она принадлежит.
    /// </remarks>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    /// <returns>Данные текущего студента.</returns>
    [HttpGet("me")]
    [Authorize(Roles = "student")]
    [ProducesResponseType(typeof(StudentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<StudentDto>> GetCurrentStudent(
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var studentId))
            return Unauthorized(ApiErrors.AuthenticationRequired);

        var student = await studentIdentityService.FindByIdAsync(studentId, cancellationToken);
        return student is null
            ? Unauthorized(ApiErrors.AuthenticationRequired)
            : Ok(student);
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
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }
}
