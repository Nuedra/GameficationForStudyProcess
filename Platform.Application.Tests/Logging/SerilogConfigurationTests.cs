using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Platform.Application.Authentication;
using Platform.Application.Controllers;
using Platform.Application.Logging;
using Platform.Application.Middleware;
using Platform.Application.Pages;
using Platform.Application.Services;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Platform.Application.Tests.Logging;

public sealed class SerilogConfigurationTests
{
    public static TheoryData<string, bool, bool> SourceContextRoutingCases => new()
    {
        { typeof(AuthController).FullName!, true, false },
        { typeof(LoginModel).FullName!, true, false },
        { typeof(PlatformCookieAuthenticationEvents).FullName!, true, false },
        { typeof(UserIdentityService).FullName!, true, false },
        { typeof(UserSessionService).FullName!, true, false },
        { typeof(AchievementManagementService).FullName!, false, true },
        { typeof(StaffCourseExportService).FullName!, false, true },
        { typeof(TeachingAssignmentService).FullName!, false, true },
        { typeof(StudentCoursesController).FullName!, false, false }
    };

    [Theory]
    [MemberData(nameof(SourceContextRoutingCases))]
    public void PlatformLogFilters_RouteExpectedSourceContexts(
        string sourceContext,
        bool expectedSecurity,
        bool expectedBusiness)
    {
        var logEvent = CreateLogEvent(sourceContext);

        Assert.Equal(expectedSecurity, PlatformLogFilters.IsSecurityEvent(logEvent));
        Assert.Equal(expectedBusiness, PlatformLogFilters.IsBusinessEvent(logEvent));
    }

    [Fact]
    public async Task RequestLogContextMiddleware_AddsRequestPropertiesOnlyInsideRequestScope()
    {
        var sink = new CapturingSink();
        using var logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Sink(sink)
            .CreateLogger();
        var context = new DefaultHttpContext
        {
            TraceIdentifier = "trace-request-1"
        };
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = "/api/test";
        context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");

        var middleware = new RequestLogContextMiddleware(_ =>
        {
            logger.Information("inside request scope");
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);
        logger.Information("outside request scope");

        Assert.Equal(2, sink.Events.Count);
        var scopedEvent = sink.Events[0];
        AssertScalar(scopedEvent, "TraceId", "trace-request-1");
        AssertScalar(scopedEvent, "RequestMethod", "POST");
        AssertScalar(scopedEvent, "RequestPath", "/api/test");
        AssertScalar(scopedEvent, "RemoteIp", "127.0.0.1");
        Assert.DoesNotContain("TraceId", sink.Events[1].Properties.Keys);
    }

    [Fact]
    public async Task AuthenticatedUserLogContextMiddleware_AddsUserPropertiesOnlyInsideUserScope()
    {
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var sink = new CapturingSink();
        using var logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Sink(sink)
            .CreateLogger();
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Role, "teacher"),
                    new Claim(PlatformClaimTypes.SessionId, sessionId.ToString())
                ],
                "TestAuthentication"))
        };

        var middleware = new AuthenticatedUserLogContextMiddleware(_ =>
        {
            logger.Information("inside authenticated user scope");
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);
        logger.Information("outside authenticated user scope");

        Assert.Equal(2, sink.Events.Count);
        var scopedEvent = sink.Events[0];
        AssertScalar(scopedEvent, "UserId", userId.ToString());
        AssertScalar(scopedEvent, "Role", "teacher");
        AssertScalar(scopedEvent, "SessionId", sessionId.ToString());
        Assert.DoesNotContain("UserId", sink.Events[1].Properties.Keys);
    }

    [Fact]
    public void SerilogRequestLogEnricher_AddsEndpointAndAuthenticatedUserProperties()
    {
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var diagnosticContext = new CapturingDiagnosticContext();
        var httpContext = new DefaultHttpContext
        {
            TraceIdentifier = "trace-request-log",
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Role, "administrator"),
                    new Claim(PlatformClaimTypes.SessionId, sessionId.ToString())
                ],
                "TestAuthentication"))
        };
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
        httpContext.SetEndpoint(new Endpoint(
            _ => Task.CompletedTask,
            new EndpointMetadataCollection(),
            "Test endpoint"));

        SerilogRequestLogEnricher.EnrichFromRequest(diagnosticContext, httpContext);

        Assert.Equal("trace-request-log", diagnosticContext.Properties["TraceId"]);
        Assert.Equal("127.0.0.1", diagnosticContext.Properties["RemoteIp"]);
        Assert.Equal("Test endpoint", diagnosticContext.Properties["EndpointName"]);
        Assert.Equal(userId.ToString(), diagnosticContext.Properties["UserId"]);
        Assert.Equal("administrator", diagnosticContext.Properties["Role"]);
        Assert.Equal(sessionId.ToString(), diagnosticContext.Properties["SessionId"]);
    }

    private static LogEvent CreateLogEvent(string sourceContext)
    {
        var sink = new CapturingSink();
        using var logger = new LoggerConfiguration()
            .WriteTo.Sink(sink)
            .CreateLogger();
        logger
            .ForContext("SourceContext", sourceContext)
            .Information("test log event");

        return Assert.Single(sink.Events);
    }

    private static void AssertScalar(
        LogEvent logEvent,
        string propertyName,
        object expectedValue)
    {
        var property = Assert.IsType<ScalarValue>(logEvent.Properties[propertyName]);
        Assert.Equal(expectedValue, property.Value);
    }

    private sealed class CapturingSink : ILogEventSink
    {
        public List<LogEvent> Events { get; } = [];

        public void Emit(LogEvent logEvent) => Events.Add(logEvent);
    }

    private sealed class CapturingDiagnosticContext : IDiagnosticContext
    {
        public Dictionary<string, object?> Properties { get; } = new(StringComparer.Ordinal);

        public Exception? Exception { get; private set; }

        public void Set(
            string propertyName,
            object? value,
            bool destructureObjects = false)
        {
            Properties[propertyName] = value;
        }

        public void SetException(Exception exception)
        {
            Exception = exception;
        }
    }
}
