namespace Platform.Application.Services;

public interface IStudentAchievementGraphService
{
    Task<StudentAchievementGraphQueryResult> GetGraphXmlAsync(
        Guid studentId,
        Guid courseId,
        int year,
        CancellationToken cancellationToken = default);

    Task<StudentAchievementGraphQueryResult> RefreshGraphXmlAsync(
        Guid studentId,
        Guid courseId,
        int year,
        CancellationToken cancellationToken = default);
}

public enum StudentAchievementGraphQueryStatus
{
    Success,
    StudentNotFound,
    CourseNotFound,
    AccessDenied,
    TemplateNotFound,
    InvalidTemplate
}

public sealed record StudentAchievementGraphQueryResult(
    StudentAchievementGraphQueryStatus Status,
    string? Xml = null);
