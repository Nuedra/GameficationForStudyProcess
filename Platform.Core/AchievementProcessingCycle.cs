using Microsoft.EntityFrameworkCore;
using Platform.Core.Appraisals;
using Platform.DataAccess.Postgress;

namespace Platform.Core.Processing;

public sealed class AchievementProcessingCycle
{
    private readonly string _connectionString;
    private readonly IAppraisalPayloadProvider _payloadProvider;
    private readonly IAppraisalFactsExtractor _factsExtractor;
    private readonly TimeProvider _timeProvider;

    public AchievementProcessingCycle(
        string connectionString,
        IAppraisalPayloadProvider payloadProvider,
        IAppraisalFactsExtractor factsExtractor,
        TimeProvider? timeProvider = null)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required.", nameof(connectionString));

        _connectionString = connectionString;
        _payloadProvider = payloadProvider ?? throw new ArgumentNullException(nameof(payloadProvider));
        _factsExtractor = factsExtractor ?? throw new ArgumentNullException(nameof(factsExtractor));
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<AchievementProcessingResult> RunAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        // подключение к БД
        await using var db = PlatformDatabase.Connect(_connectionString);

        // ищет студента и проверяет его наличие
        var studentExists = await db.Students
            .AsNoTracking()
            .AnyAsync(student => student.Id == studentId, cancellationToken);

        if (!studentExists)
            throw new InvalidOperationException($"Student with id {studentId} was not found.");

        // поиск ДТО нужного студента
        var payloads = await _payloadProvider.GetPayloadsAsync(cancellationToken);
        var facts = payloads
            .Where(payload => payload.StudentId == studentId)
            .Select(_factsExtractor.Extract)
            .ToList();

        // загрузка достижений только для курсов и годов из ДТО студента
        var achievementEntities = new List<AchievementEntity>();
        var courseYears = facts
            .Select(courseFacts => (courseFacts.CourseId, courseFacts.Year))
            .Distinct()
            .ToList();

        foreach (var (courseId, year) in courseYears)
        {
            achievementEntities.AddRange(await db.Achievements
                .AsNoTracking()
                .Include(achievement => achievement.Criteria)
                .Where(achievement =>
                    achievement.CourseID == courseId &&
                    achievement.Year == year)
                .ToListAsync(cancellationToken));
        }

        var matchedEntities = new List<AchievementEntity>();
        var checkedCount = 0;

        // перебор всех загруженных достижений
        foreach (var achievement in achievementEntities)
        {
            var criteria = achievement.Criteria;
            if (criteria == null || !criteria.IsEnabled)
                continue;

            checkedCount++;

            if (facts.Any(courseFacts => IsMatch(achievement, courseFacts)))
                matchedEntities.Add(achievement);
        }

        var matchedIds = matchedEntities.Select(achievement => achievement.Id).ToList();
        var existingAchievementIds = (await db.StudentAchievements
            .AsNoTracking()
            .Where(item => item.StudentID == studentId)
            .Select(item => item.AchievementID)
            .ToListAsync(cancellationToken))
            .ToHashSet();

        var connections = await db.AchievementConnections
            .AsNoTracking()
            .Where(connection => matchedIds.Contains(connection.TargetId))
            .Select(connection => new AchievementDependency(connection.SourceId, connection.TargetId))
            .ToListAsync(cancellationToken);

        var assignableEntities = ResolveDependencies(
            matchedEntities.Where(achievement => !existingAchievementIds.Contains(achievement.Id)),
            existingAchievementIds,
            connections);

        // запись в бд новых полученных достижений
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var assignedEntities = assignableEntities
            .Select(achievement => new StudentAchievementEntity
            {
                Id = Guid.NewGuid(),
                StudentID = studentId,
                AchievementID = achievement.Id,
                AchievementGotDate = GetAchievementGotDate(achievement, facts, now),
                AchievementFoundDate = now,
                IsNotificationSeen = false,
                IsFirstAnimationShown = false
            })
            .ToList();

        if (assignedEntities.Count > 0)
        {
            db.StudentAchievements.AddRange(assignedEntities);
            await db.SaveChangesAsync(cancellationToken);
        }

        var matched = matchedEntities
            .Select(entity => new ProcessedAchievement(entity.Id, entity.Title))
            .ToList();
        var assignedIds = assignedEntities.Select(entity => entity.AchievementID).ToHashSet();
        var assigned = matched.Where(achievement => assignedIds.Contains(achievement.Id)).ToList();
        var dependencyBlocked = matched
            .Where(achievement =>
                !existingAchievementIds.Contains(achievement.Id) &&
                !assignedIds.Contains(achievement.Id))
            .ToList();

        // возвращает результат работы цикла
        return new AchievementProcessingResult(
            TotalAchievements: achievementEntities.Count,
            CheckedAchievements: checkedCount,
            MatchedAchievements: matched,
            AssignedAchievements: assigned,
            DependencyBlockedAchievements: dependencyBlocked
        );
    }

    private static IReadOnlyList<AchievementEntity> ResolveDependencies(
        IEnumerable<AchievementEntity> candidates,
        IReadOnlySet<Guid> existingAchievementIds,
        IReadOnlyList<AchievementDependency> connections)
    {
        var availableAchievementIds = existingAchievementIds.ToHashSet();
        var pending = candidates.ToDictionary(achievement => achievement.Id);
        var dependenciesByTarget = connections
            .GroupBy(connection => connection.TargetId)
            .ToDictionary(
                group => group.Key,
                group => group.Select(connection => connection.SourceId).ToHashSet());
        var assignable = new List<AchievementEntity>();

        while (pending.Count > 0)
        {
            var unlocked = pending.Values
                .Where(achievement =>
                    !dependenciesByTarget.TryGetValue(achievement.Id, out var dependencies) ||
                    dependencies.Any(availableAchievementIds.Contains))
                .ToList();

            if (unlocked.Count == 0)
                break;

            foreach (var achievement in unlocked)
            {
                pending.Remove(achievement.Id);
                availableAchievementIds.Add(achievement.Id);
                assignable.Add(achievement);
            }
        }

        return assignable;
    }

    private static bool IsMatch(AchievementEntity achievement, StudentCourseFacts facts)
    {
        return achievement.CourseID == facts.CourseId &&
               achievement.Year == facts.Year &&
               facts.Marks.Any(mark => AchievementTagMatcher.IsMatch(achievement.Criteria.Expression, mark.Tags));
    }

    private static DateTime GetAchievementGotDate(
        AchievementEntity achievement,
        IReadOnlyList<StudentCourseFacts> facts,
        DateTime fallback)
    {
        var requiredTags = AchievementTagMatcher.ParseExpression(achievement.Criteria.Expression);

        return facts
            .Where(courseFacts =>
                courseFacts.CourseId == achievement.CourseID &&
                courseFacts.Year == achievement.Year)
            .SelectMany(courseFacts => courseFacts.Marks)
            .Where(mark => requiredTags.All(requiredTag =>
                mark.Tags.Contains(requiredTag, StringComparer.Ordinal)))
            .Select(mark => mark.UploadedAt ?? mark.UpdatedAt)
            .Where(date => date.HasValue)
            .Select(date => date!.Value.UtcDateTime)
            .DefaultIfEmpty(fallback)
            .Min();
    }
}

public sealed record AchievementProcessingResult(
    int TotalAchievements,
    int CheckedAchievements,
    IReadOnlyList<ProcessedAchievement> MatchedAchievements,
    IReadOnlyList<ProcessedAchievement> AssignedAchievements,
    IReadOnlyList<ProcessedAchievement> DependencyBlockedAchievements
);

public sealed record ProcessedAchievement(Guid Id, string Title);

public sealed record AchievementDependency(Guid SourceId, Guid TargetId);
