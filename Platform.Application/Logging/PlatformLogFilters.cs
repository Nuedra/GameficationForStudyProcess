using Serilog.Events;

namespace Platform.Application.Logging;

internal static class PlatformLogFilters
{
    private static readonly string[] SecuritySources =
    [
        "Platform.Application.Authentication.",
        "Platform.Application.Controllers.AuthController",
        "Platform.Application.Pages.LoginModel",
        "Platform.Application.Services.UserIdentityService",
        "Platform.Application.Services.UserSessionService"
    ];

    private static readonly string[] BusinessSources =
    [
        "Platform.Application.Services.AchievementManagementService",
        "Platform.Application.Services.TeachingAssignmentService"
    ];

    public static bool IsSecurityEvent(LogEvent logEvent) =>
        HasSourcePrefix(logEvent, SecuritySources);

    public static bool IsBusinessEvent(LogEvent logEvent) =>
        HasSourcePrefix(logEvent, BusinessSources);

    private static bool HasSourcePrefix(
        LogEvent logEvent,
        IReadOnlyCollection<string> sourcePrefixes)
    {
        if (!logEvent.Properties.TryGetValue("SourceContext", out var sourceProperty) ||
            sourceProperty is not ScalarValue { Value: string sourceContext })
        {
            return false;
        }

        return sourcePrefixes.Any(prefix =>
            sourceContext.StartsWith(prefix, StringComparison.Ordinal));
    }
}
