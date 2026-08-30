using System.Security.Claims;
using Platform.Application.Authentication;
using Serilog;

namespace Platform.Application.Logging;

internal static class SerilogRequestLogEnricher
{
    public static void EnrichFromRequest(
        IDiagnosticContext diagnosticContext,
        HttpContext httpContext)
    {
        diagnosticContext.Set("TraceId", httpContext.TraceIdentifier);
        diagnosticContext.Set("RemoteIp", httpContext.Connection.RemoteIpAddress?.ToString());

        var endpointName = httpContext.GetEndpoint()?.DisplayName;
        if (!string.IsNullOrWhiteSpace(endpointName))
            diagnosticContext.Set("EndpointName", endpointName);

        if (httpContext.User.Identity?.IsAuthenticated != true)
            return;

        diagnosticContext.Set("UserId", httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
        diagnosticContext.Set("Role", httpContext.User.FindFirstValue(ClaimTypes.Role));
        diagnosticContext.Set(
            "SessionId",
            httpContext.User.FindFirstValue(PlatformClaimTypes.SessionId));
    }
}
