using Platform.Application.Contracts;

namespace Platform.Application.Services;

public interface IStudentStatisticsService
{
    Task<StudentStatisticsQueryResult> GetCurrentCoursesStatisticsAsync(
        Guid studentId,
        CancellationToken cancellationToken = default);

    Task<StudentStatisticsQueryResult> GetCourseStatisticsAsync(
        Guid studentId,
        Guid courseId,
        int year,
        CancellationToken cancellationToken = default);
}

public enum StudentStatisticsQueryStatus
{
    Success,
    StudentNotFound,
    CourseNotFound,
    AccessDenied
}

public sealed record StudentStatisticsQueryResult(
    StudentStatisticsQueryStatus Status,
    StudentStatisticsDto? Statistics = null);
