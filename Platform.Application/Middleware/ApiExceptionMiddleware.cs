using Platform.Application.Contracts;

namespace Platform.Application.Middleware;

/// <summary>
/// Не позволяет необработанным исключениям API попадать в браузер вместе с
/// техническими деталями. Полная причина остаётся только в журнале сервера.
/// </summary>
public sealed class ApiExceptionMiddleware(
    RequestDelegate next,
    ILogger<ApiExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception) when (IsApiRequest(context) && !context.Response.HasStarted)
        {
            logger.LogError(
                exception,
                "Unhandled API error. Exception type: {ExceptionType}; method: {Method}; path: {Path}; trace id: {TraceId}",
                exception.GetType().FullName,
                context.Request.Method,
                context.Request.Path,
                context.TraceIdentifier);

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(ApiErrors.InternalServerError);
        }
    }

    private static bool IsApiRequest(HttpContext context) =>
        context.Request.Path.StartsWithSegments("/api");
}
