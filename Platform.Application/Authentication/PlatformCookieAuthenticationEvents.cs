using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Platform.Application.Contracts;
using Platform.Application.Services;

namespace Platform.Application.Authentication;

public sealed class PlatformCookieAuthenticationEvents(
    IUserIdentityService userIdentityService,
    ILogger<PlatformCookieAuthenticationEvents> logger) : CookieAuthenticationEvents
{
    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        if (context.Principal is null ||
            !context.Principal.TryGetUserId(out var userId) ||
            !context.Principal.TryGetUserRole(out var claimedRole))
        {
            await RejectSessionAsync(context, null, "claims_invalid");
            return;
        }

        var currentIdentity = await userIdentityService.ResolveByIdAsync(
            userId,
            context.HttpContext.RequestAborted);
        if (currentIdentity is null || currentIdentity.Role != claimedRole)
        {
            await RejectSessionAsync(context, userId, "identity_changed");
        }
    }

    public override Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return context.Response.WriteAsJsonAsync(ApiErrors.AuthenticationRequired);
    }

    public override Task RedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> context)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return context.Response.WriteAsJsonAsync(ApiErrors.AccessDenied);
    }

    private async Task RejectSessionAsync(
        CookieValidatePrincipalContext context,
        Guid? userId,
        string reason)
    {
        logger.LogWarning(
            "Authentication session rejected. UserId={UserId}, Reason={Reason}, RemoteIp={RemoteIp}",
            userId,
            reason,
            context.HttpContext.Connection.RemoteIpAddress);
        context.RejectPrincipal();
        await context.HttpContext.SignOutAsync(context.Scheme.Name);
    }
}
