using Microsoft.EntityFrameworkCore;
using Platform.Core.AchievementGraphs;
using Platform.Core.Processing;
using Platform.DataAccess.Postgress;

namespace Platform.Application.Services;

public sealed class StudentAchievementGraphService(
    PlatformDbContext dbContext,
    TimeProvider timeProvider,
    IAchievementGraphTemplateProvider templateProvider,
    IAchievementGraphXmlSerializer serializer) : IStudentAchievementGraphService
{
    public async Task<StudentAchievementGraphQueryResult> GetGraphXmlAsync(
        Guid studentId,
        Guid courseId,
        int year,
        CancellationToken cancellationToken = default)
    {
        var accessStatus = await CheckAccessAsync(
            studentId,
            courseId,
            year,
            cancellationToken);

        if (accessStatus != StudentAchievementsQueryStatus.Success)
            return ToGraphResult(accessStatus);

        var nodeStates = await GetNodeStatesAsync(
            studentId,
            courseId,
            year,
            cancellationToken);

        string template;
        try
        {
            template = await templateProvider.GetTemplateAsync(cancellationToken);
        }
        catch (Exception exception) when (
            exception is FileNotFoundException or DirectoryNotFoundException)
        {
            return new StudentAchievementGraphQueryResult(
                StudentAchievementGraphQueryStatus.TemplateNotFound);
        }

        try
        {
            var xml = serializer.Serialize(template, nodeStates);
            return new StudentAchievementGraphQueryResult(
                StudentAchievementGraphQueryStatus.Success,
                xml);
        }
        catch (AchievementGraphXmlException)
        {
            return new StudentAchievementGraphQueryResult(
                StudentAchievementGraphQueryStatus.InvalidTemplate);
        }
    }

    private async Task<StudentAchievementsQueryStatus> CheckAccessAsync(
        Guid studentId,
        Guid courseId,
        int year,
        CancellationToken cancellationToken)
    {
        var studentExists = await dbContext.Students
            .AsNoTracking()
            .AnyAsync(student => student.Id == studentId, cancellationToken);

        if (!studentExists)
            return StudentAchievementsQueryStatus.StudentNotFound;

        var courseExists = await dbContext.CourseInstances
            .AsNoTracking()
            .AnyAsync(
                course => course.CourseID == courseId && course.Year == year,
                cancellationToken);

        if (!courseExists)
            return StudentAchievementsQueryStatus.CourseNotFound;

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

        return hasCourseAccess
            ? StudentAchievementsQueryStatus.Success
            : StudentAchievementsQueryStatus.AccessDenied;
    }

    private async Task<IReadOnlyList<AchievementGraphNodeState>> GetNodeStatesAsync(
        Guid studentId,
        Guid courseId,
        int year,
        CancellationToken cancellationToken)
    {
        var achievements = await dbContext.Achievements
            .AsNoTracking()
            .Where(achievement =>
                achievement.CourseID == courseId &&
                achievement.Year == year)
            .Select(achievement => achievement.Id)
            .ToListAsync(cancellationToken);

        var achievementIds = achievements
            .ToHashSet();

        var earnedIds = (await dbContext.StudentAchievements
                .AsNoTracking()
                .Where(studentAchievement =>
                    studentAchievement.StudentID == studentId &&
                    achievementIds.Contains(studentAchievement.AchievementID))
                .Select(studentAchievement => studentAchievement.AchievementID)
                .ToListAsync(cancellationToken))
            .ToHashSet();

        var dependencies = await dbContext.AchievementConnections
            .AsNoTracking()
            .Where(connection =>
                achievementIds.Contains(connection.SourceId) &&
                achievementIds.Contains(connection.TargetId))
            .Select(connection => new AchievementDependency(
                connection.SourceId,
                connection.TargetId))
            .ToListAsync(cancellationToken);

        var dependenciesByTarget = dependencies
            .GroupBy(dependency => dependency.TargetId)
            .ToDictionary(
                group => group.Key,
                group => group.Select(dependency => dependency.SourceId).ToHashSet());

        return achievements
            .Select(achievement => new AchievementGraphNodeState(
                achievement,
                ResolveStatus(achievement, earnedIds, dependenciesByTarget)))
            .ToList();
    }

    private static AchievementGraphStatus ResolveStatus(
        Guid achievementId,
        IReadOnlySet<Guid> earnedIds,
        IReadOnlyDictionary<Guid, HashSet<Guid>> dependenciesByTarget)
    {
        if (earnedIds.Contains(achievementId))
            return AchievementGraphStatus.Earned;

        if (!dependenciesByTarget.TryGetValue(achievementId, out var dependencies) ||
            dependencies.Any(earnedIds.Contains))
        {
            return AchievementGraphStatus.Available;
        }

        return AchievementGraphStatus.Locked;
    }

    private static StudentAchievementGraphQueryResult ToGraphResult(
        StudentAchievementsQueryStatus status)
    {
        return status switch
        {
            StudentAchievementsQueryStatus.StudentNotFound =>
                new StudentAchievementGraphQueryResult(
                    StudentAchievementGraphQueryStatus.StudentNotFound),
            StudentAchievementsQueryStatus.CourseNotFound =>
                new StudentAchievementGraphQueryResult(
                    StudentAchievementGraphQueryStatus.CourseNotFound),
            StudentAchievementsQueryStatus.AccessDenied =>
                new StudentAchievementGraphQueryResult(
                    StudentAchievementGraphQueryStatus.AccessDenied),
            _ => new StudentAchievementGraphQueryResult(
                StudentAchievementGraphQueryStatus.InvalidTemplate)
        };
    }
}
