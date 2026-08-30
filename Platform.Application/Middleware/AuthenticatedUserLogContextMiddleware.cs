using System.Security.Claims;
using Platform.Application.Authentication;
using Serilog.Context;

namespace Platform.Application.Middleware;

public sealed class AuthenticatedUserLogContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await next(context);
            return;
        }

        using var userId = LogContext.PushProperty(
            "UserId",
            context.User.FindFirstValue(ClaimTypes.NameIdentifier));
        using var role = LogContext.PushProperty(
            "Role",
            context.User.FindFirstValue(ClaimTypes.Role));
        using var sessionId = LogContext.PushProperty(
            "SessionId",
            context.User.FindFirstValue(PlatformClaimTypes.SessionId));

        await next(context);
    }
}
