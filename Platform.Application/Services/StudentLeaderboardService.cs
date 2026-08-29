using Microsoft.EntityFrameworkCore;
using Platform.Application.Contracts;
using Platform.DataAccess.Postgress;
using Platform.Lms;

namespace Platform.Application.Services;

public sealed class StudentLeaderboardService(
    AchievementDbContext dbContext,
    ILmsDataSource lmsDataSource,
    TimeProvider timeProvider) : IStudentLeaderboardService
{
    public async Task<StudentLeaderboardQueryResult> GetLeaderboardAsync(
        Guid studentId,
        Guid courseId,
        int year,
        CancellationToken cancellationToken = default)
    {
        var effectiveAt = timeProvider.GetUtcNow();

        var studentExists = await lmsDataSource.GetPersonAsync(
            studentId,
            cancellationToken) is not null;
        if (!studentExists)
            return new StudentLeaderboardQueryResult(
                StudentLeaderboardQueryStatus.StudentNotFound);

        var courseExists = await lmsDataSource.CourseInstanceExistsAsync(
            courseId,
            year,
            cancellationToken);
        if (!courseExists)
            return new StudentLeaderboardQueryResult(
                StudentLeaderboardQueryStatus.CourseNotFound);

        var hasCourseAccess = await lmsDataSource.HasActiveEnrollmentAsync(
            studentId,
            courseId,
            year,
            effectiveAt,
            cancellationToken);
        if (!hasCourseAccess)
            return new StudentLeaderboardQueryResult(
                StudentLeaderboardQueryStatus.AccessDenied);

        var students = await lmsDataSource.GetActiveCourseInstanceStudentsAsync(
            courseId,
            year,
            effectiveAt,
            cancellationToken);

        var studentIds = students.Select(student => student.Id).ToHashSet();
        var achievementIds = await dbContext.Achievements
            .AsNoTracking()
            .Where(achievement =>
                achievement.CourseID == courseId &&
                achievement.Year == year)
            .Select(achievement => achievement.Id)
            .ToListAsync(cancellationToken);

        var achievementIdSet = achievementIds.ToHashSet();
        var countsByStudent = achievementIdSet.Count == 0 || studentIds.Count == 0
            ? new Dictionary<Guid, int>()
            : await dbContext.StudentAchievements
                .AsNoTracking()
                .Where(studentAchievement =>
                    studentIds.Contains(studentAchievement.StudentID) &&
                    achievementIdSet.Contains(studentAchievement.AchievementID))
                .GroupBy(studentAchievement => studentAchievement.StudentID)
                .Select(group => new
                {
                    StudentId = group.Key,
                    AchievementCount = group.Count()
                })
                .ToDictionaryAsync(
                    item => item.StudentId,
                    item => item.AchievementCount,
                    cancellationToken);

        var entries = students
            .Select(student => new LeaderboardEntryDto(
                student.Id,
                student.DisplayName,
                student.CurrentEducationalGroupName,
                countsByStudent.GetValueOrDefault(student.Id)))
            .OrderByDescending(entry => entry.AchievementCount)
            .ThenBy(entry => entry.StudentName, StringComparer.Ordinal)
            .ThenBy(entry => entry.Group, StringComparer.Ordinal)
            .ToList();

        return new StudentLeaderboardQueryResult(
            StudentLeaderboardQueryStatus.Success,
            entries);
    }
}
