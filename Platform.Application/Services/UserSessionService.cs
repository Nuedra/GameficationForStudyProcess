using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Platform.Application.Authentication;
using Platform.Application.Contracts;
using Platform.Core.Models;

namespace Platform.Application.Services;

public sealed class UserSessionService(
    IUserIdentityService userIdentityService,
    TimeProvider timeProvider,
    ILogger<UserSessionService> logger) : IUserSessionService
{
    public async Task<AuthenticatedUserDto?> SignInAsync(
        HttpContext httpContext,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await userIdentityService.ResolveByIdAsync(userId, cancellationToken);
        if (user is null)
            return null;

        var sessionId = Guid.NewGuid();
        var issuedUtc = timeProvider.GetUtcNow();
        var expiresUtc = issuedUtc.AddHours(8);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.DisplayName),
            new(ClaimTypes.Role, UserRoleDictionary.Values[user.Role]),
            new(PlatformClaimTypes.SessionId, sessionId.ToString())
        };
        if (!string.IsNullOrWhiteSpace(user.Group))
            claims.Add(new Claim(PlatformClaimTypes.Group, user.Group));

        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme));
        await httpContext.SignInAsync(
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
            httpContext.Connection.RemoteIpAddress);

        return new AuthenticatedUserDto(user.Id, user.DisplayName, user.Role, user.Group);
    }

    public async Task SignOutAsync(HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = httpContext.User.FindFirstValue(ClaimTypes.Role);
        var sessionId = httpContext.User.FindFirstValue(PlatformClaimTypes.SessionId);
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        logger.LogInformation(
            "Authentication session ended. UserId={UserId}, Role={Role}, SessionId={SessionId}, RemoteIp={RemoteIp}",
            userId,
            role,
            sessionId,
            httpContext.Connection.RemoteIpAddress);
    }
}
