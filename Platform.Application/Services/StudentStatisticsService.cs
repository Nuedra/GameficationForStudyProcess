using Microsoft.EntityFrameworkCore;
using Platform.Application.Contracts;
using Platform.DataAccess.Postgress;
using Platform.Lms;

namespace Platform.Application.Services;

public sealed class StudentStatisticsService(
    AchievementDbContext dbContext,
    ILmsDataSource lmsDataSource,
    TimeProvider timeProvider) : IStudentStatisticsService
{
    public async Task<StudentStatisticsQueryResult> GetCurrentCoursesStatisticsAsync(
        Guid studentId,
        CancellationToken cancellationToken = default)
    {
        if (await lmsDataSource.GetPersonAsync(studentId, cancellationToken) is null)
            return new StudentStatisticsQueryResult(StudentStatisticsQueryStatus.StudentNotFound);

        var courses = await lmsDataSource.GetActiveCourseInstancesAsync(
            studentId,
            timeProvider.GetUtcNow(),
            cancellationToken);
        var scopes = courses
            .Select(course => new CourseScope(course.CourseId, course.Year))
            .ToHashSet();

        return await BuildStatisticsAsync(studentId, scopes, cancellationToken);
    }

    public async Task<StudentStatisticsQueryResult> GetCourseStatisticsAsync(
        Guid studentId,
        Guid courseId,
        int year,
        CancellationToken cancellationToken = default)
    {
        if (await lmsDataSource.GetPersonAsync(studentId, cancellationToken) is null)
            return new StudentStatisticsQueryResult(StudentStatisticsQueryStatus.StudentNotFound);

        if (!await lmsDataSource.CourseInstanceExistsAsync(courseId, year, cancellationToken))
            return new StudentStatisticsQueryResult(StudentStatisticsQueryStatus.CourseNotFound);

        var hasActiveEnrollment = await lmsDataSource.HasActiveEnrollmentAsync(
            studentId,
            courseId,
            year,
            timeProvider.GetUtcNow(),
            cancellationToken);
        if (!hasActiveEnrollment)
            return new StudentStatisticsQueryResult(StudentStatisticsQueryStatus.AccessDenied);

        return await BuildStatisticsAsync(
            studentId,
            new HashSet<CourseScope> { new(courseId, year) },
            cancellationToken);
    }

    private async Task<StudentStatisticsQueryResult> BuildStatisticsAsync(
        Guid studentId,
        IReadOnlySet<CourseScope> scopes,
        CancellationToken cancellationToken)
    {
        var courseIds = scopes.Select(scope => scope.CourseId).ToHashSet();
        var awards = courseIds.Count == 0
            ? []
            : await dbContext.StudentAchievements
                .AsNoTracking()
                .Where(award =>
                    award.StudentID == studentId &&
                    courseIds.Contains(award.Achievement.CourseID))
                .Select(award => new AwardStatisticsItem(
                    award.Achievement.CourseID,
                    award.Achievement.Year,
                    award.Achievement.Rarity))
                .ToListAsync(cancellationToken);

        var counts = awards
            .Where(award => scopes.Contains(new CourseScope(award.CourseId, award.Year)))
            .GroupBy(award => award.Rarity)
            .ToDictionary(group => group.Key, group => group.Count());
        var byRarity = Enum.GetValues<AchievementRarity>()
            .Select(rarity => new AchievementRarityCountDto(
                rarity,
                counts.GetValueOrDefault(rarity)))
            .ToList();

        return new StudentStatisticsQueryResult(
            StudentStatisticsQueryStatus.Success,
            new StudentStatisticsDto(byRarity.Sum(item => item.Count), byRarity));
    }

    private readonly record struct CourseScope(Guid CourseId, int Year);

    private sealed record AwardStatisticsItem(
        Guid CourseId,
        int Year,
        AchievementRarity Rarity);
}
