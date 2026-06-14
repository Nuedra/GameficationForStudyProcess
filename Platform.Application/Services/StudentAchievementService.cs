using Microsoft.EntityFrameworkCore;
using Platform.Application.Contracts;
using Platform.DataAccess.Postgress;

namespace Platform.Application.Services;

public sealed class StudentAchievementService(
    PlatformDbContext dbContext,
    TimeProvider timeProvider) : IStudentAchievementService
{
    public async Task<StudentAchievementsQueryResult> GetEarnedAchievementsAsync(
        Guid studentId,
        Guid courseId,
        int year,
        CancellationToken cancellationToken = default)
    {
        var student = await dbContext.Students
            .AsNoTracking()
            .Where(item => item.Id == studentId)
            .Select(item => new StudentDto(
                item.Id,
                item.Surname + " " + item.Name,
                item.Group))
            .SingleOrDefaultAsync(cancellationToken);

        if (student is null)
        {
            return new StudentAchievementsQueryResult(
                StudentAchievementsQueryStatus.StudentNotFound);
        }

        var course = await dbContext.CourseInstances
            .AsNoTracking()
            .Where(item => item.CourseID == courseId && item.Year == year)
            .Select(item => new CourseDto(
                item.CourseID,
                item.Course.Title,
                item.Course.Description,
                item.Year))
            .SingleOrDefaultAsync(cancellationToken);

        if (course is null)
        {
            return new StudentAchievementsQueryResult(
                StudentAchievementsQueryStatus.CourseNotFound);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var hasCourseAccess = await dbContext.CourseInstanceStudents
            .AsNoTracking()
            .AnyAsync(
                item =>
                    item.PersonID == studentId &&
                    item.CourseID == courseId &&
                    item.Year == year &&
                    item.StartDate <= now &&
                    (!item.EndDate.HasValue || item.EndDate.Value >= now),
                cancellationToken);

        if (!hasCourseAccess)
        {
            return new StudentAchievementsQueryResult(
                StudentAchievementsQueryStatus.AccessDenied);
        }

        var earnedData = await dbContext.StudentAchievements
            .AsNoTracking()
            .Where(item =>
                item.StudentID == studentId &&
                item.Achievement.CourseID == courseId &&
                item.Achievement.Year == year)
            .OrderBy(item => item.AchievementGotDate)
            .Select(item => new EarnedAchievementData(
                item.AchievementID,
                item.Achievement.Title,
                item.Achievement.Description,
                item.AchievementGotDate))
            .ToListAsync(cancellationToken);

        var achievements = earnedData
            .Select(item => new AchievementDto(
                item.Id,
                item.Name,
                item.Description,
                AchievementStatus.Earned,
                ToDateTimeOffset(item.EarnedAt),
                null))
            .ToList();

        return new StudentAchievementsQueryResult(
            StudentAchievementsQueryStatus.Success,
            new StudentAchievementsDto(student, course, achievements));
    }

    private static DateTimeOffset ToDateTimeOffset(DateTime value)
    {
        var utcValue = value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);

        return new DateTimeOffset(utcValue);
    }

    private sealed record EarnedAchievementData(
        Guid Id,
        string Name,
        string Description,
        DateTime EarnedAt);
}
