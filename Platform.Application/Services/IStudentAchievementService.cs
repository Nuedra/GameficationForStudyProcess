using Platform.Application.Contracts;

namespace Platform.Application.Services;

public interface IStudentAchievementService
{
    Task<StudentAchievementsQueryResult> GetEarnedAchievementsAsync(
        Guid studentId,
        Guid courseId,
        int year,
        CancellationToken cancellationToken = default);
}

public enum StudentAchievementsQueryStatus
{
    Success,
    StudentNotFound,
    CourseNotFound,
    AccessDenied
}

public sealed record StudentAchievementsQueryResult(
    StudentAchievementsQueryStatus Status,
    StudentAchievementsDto? Data = null);
