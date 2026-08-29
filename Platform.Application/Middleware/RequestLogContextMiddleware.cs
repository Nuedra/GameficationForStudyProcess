using Serilog.Context;

namespace Platform.Application.Middleware;

public sealed class RequestLogContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        using var traceId = LogContext.PushProperty("TraceId", context.TraceIdentifier);
        using var requestMethod = LogContext.PushProperty("RequestMethod", context.Request.Method);
        using var requestPath = LogContext.PushProperty("RequestPath", context.Request.Path.Value);
        using var remoteIp = LogContext.PushProperty(
            "RemoteIp",
            context.Connection.RemoteIpAddress?.ToString());

        await next(context);
    }
}
