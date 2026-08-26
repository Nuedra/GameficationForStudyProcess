using Microsoft.EntityFrameworkCore;
using Platform.Core.AchievementGraphs;
using Platform.Core.Processing;
using Platform.DataAccess.Postgress;
using Platform.Lms;

namespace Platform.Application.Services;

public sealed class StudentAchievementGraphService(
    PlatformDbContext dbContext,
    ILmsDataSource lmsDataSource,
    TimeProvider timeProvider,
    IAchievementGraphTemplateProvider templateProvider,
    IAchievementGraphXmlSerializer serializer,
    AchievementProcessingCycle achievementProcessingCycle) : IStudentAchievementGraphService
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

        if (accessStatus != StudentAchievementGraphAccessStatus.Success)
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

    public async Task<StudentAchievementGraphQueryResult> RefreshGraphXmlAsync(
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

        if (accessStatus != StudentAchievementGraphAccessStatus.Success)
            return ToGraphResult(accessStatus);

        await achievementProcessingCycle.RunAsync(studentId, cancellationToken);

        return await GetGraphXmlAsync(
            studentId,
            courseId,
            year,
            cancellationToken);
    }

    private async Task<StudentAchievementGraphAccessStatus> CheckAccessAsync(
        Guid studentId,
        Guid courseId,
        int year,
        CancellationToken cancellationToken)
    {
        var studentExists = await lmsDataSource.GetPersonAsync(
            studentId,
            cancellationToken) is not null;

        if (!studentExists)
            return StudentAchievementGraphAccessStatus.StudentNotFound;

        var courseExists = await lmsDataSource.CourseInstanceExistsAsync(
            courseId,
            year,
            cancellationToken);

        if (!courseExists)
            return StudentAchievementGraphAccessStatus.CourseNotFound;

        var hasCourseAccess = await lmsDataSource.HasActiveEnrollmentAsync(
            studentId,
            courseId,
            year,
            timeProvider.GetUtcNow(),
            cancellationToken);

        return hasCourseAccess
            ? StudentAchievementGraphAccessStatus.Success
            : StudentAchievementGraphAccessStatus.AccessDenied;
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
        StudentAchievementGraphAccessStatus status)
    {
        return status switch
        {
            StudentAchievementGraphAccessStatus.StudentNotFound =>
                new StudentAchievementGraphQueryResult(
                    StudentAchievementGraphQueryStatus.StudentNotFound),
            StudentAchievementGraphAccessStatus.CourseNotFound =>
                new StudentAchievementGraphQueryResult(
                    StudentAchievementGraphQueryStatus.CourseNotFound),
            StudentAchievementGraphAccessStatus.AccessDenied =>
                new StudentAchievementGraphQueryResult(
                    StudentAchievementGraphQueryStatus.AccessDenied),
            _ => new StudentAchievementGraphQueryResult(
                StudentAchievementGraphQueryStatus.InvalidTemplate)
        };
    }

    private enum StudentAchievementGraphAccessStatus
    {
        Success,
        StudentNotFound,
        CourseNotFound,
        AccessDenied
    }
}
