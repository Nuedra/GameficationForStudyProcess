using Platform.Application.Contracts;

namespace Platform.Application.Services;

public interface IStudentLeaderboardService
{
    Task<StudentLeaderboardQueryResult> GetLeaderboardAsync(
        Guid studentId,
        Guid courseId,
        int year,
        CancellationToken cancellationToken = default);
}

public enum StudentLeaderboardQueryStatus
{
    Success,
    StudentNotFound,
    CourseNotFound,
    AccessDenied
}

public sealed record StudentLeaderboardQueryResult(
    StudentLeaderboardQueryStatus Status,
    IReadOnlyList<LeaderboardEntryDto>? Entries = null);
