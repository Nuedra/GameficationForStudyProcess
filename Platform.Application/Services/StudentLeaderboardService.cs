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
        var rarityCounts = achievementIdSet.Count == 0 || studentIds.Count == 0
            ? []
            : await dbContext.StudentAchievements
                .AsNoTracking()
                .Where(studentAchievement =>
                    studentIds.Contains(studentAchievement.StudentID) &&
                    achievementIdSet.Contains(studentAchievement.AchievementID))
                .GroupBy(studentAchievement => new
                {
                    StudentId = studentAchievement.StudentID,
                    studentAchievement.Achievement.Rarity
                })
                .Select(group => new
                    StudentAchievementRarityCount(
                        group.Key.StudentId,
                        group.Key.Rarity,
                        group.Count()))
                .ToListAsync(cancellationToken);

        var countsByStudent = rarityCounts
            .GroupBy(item => item.StudentId)
            .ToDictionary(
                group => group.Key,
                group => group.ToDictionary(item => item.Rarity, item => item.Count));

        var candidates = students.Select(student =>
        {
            var counts = countsByStudent.GetValueOrDefault(student.Id) ?? [];
            return new LeaderboardCandidate(
                new LeaderboardEntryDto(
                    student.Id,
                    student.DisplayName,
                    student.CurrentEducationalGroupName,
                    counts.Values.Sum()),
                counts);
        });

        var entries = LeaderboardOrdering.Order(
                candidates,
                candidate => candidate.Entry.AchievementCount,
                (candidate, rarity) => candidate.RarityCounts.GetValueOrDefault(rarity),
                candidate => candidate.Entry.StudentName,
                candidate => candidate.Entry.Group,
                candidate => candidate.Entry.StudentId)
            .Select(candidate => candidate.Entry)
            .ToList();

        return new StudentLeaderboardQueryResult(
            StudentLeaderboardQueryStatus.Success,
            entries);
    }

    private sealed record StudentAchievementRarityCount(
        Guid StudentId,
        AchievementRarity Rarity,
        int Count);

    private sealed record LeaderboardCandidate(
        LeaderboardEntryDto Entry,
        IReadOnlyDictionary<AchievementRarity, int> RarityCounts);
}
