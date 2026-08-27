using Microsoft.EntityFrameworkCore;
using Platform.Application.Contracts;
using Platform.Core.Abstractions;
using Platform.Core.Appraisals;
using Platform.Core.Models;
using Platform.DataAccess.Postgress;

namespace Platform.Application.Services;

public sealed class AchievementManagementService(
    AchievementDbContext dbContext,
    IStaffCourseService staffCourseService,
    IAccessPolicyService accessPolicy,
    ILogger<AchievementManagementService> logger) : IAchievementManagementService
{
    public async Task<AchievementManagementResult> GetAllAsync(
        Guid userId,
        UserRole role,
        Guid courseId,
        int year,
        CancellationToken cancellationToken = default)
    {
        var accessStatus = await GetAccessStatusAsync(
            userId,
            role,
            courseId,
            year,
            cancellationToken);
        if (accessStatus != AchievementManagementStatus.Success)
            return new AchievementManagementResult(accessStatus);

        return new AchievementManagementResult(
            AchievementManagementStatus.Success,
            await LoadDtosAsync(courseId, year, cancellationToken));
    }

    public async Task<AchievementManagementResult> CreateAsync(
        Guid userId,
        UserRole role,
        Guid courseId,
        int year,
        SaveAchievementRequest request,
        CancellationToken cancellationToken = default)
    {
        var accessStatus = await GetAccessStatusAsync(
            userId,
            role,
            courseId,
            year,
            cancellationToken);
        if (accessStatus != AchievementManagementStatus.Success)
            return new AchievementManagementResult(accessStatus);

        var normalized = Normalize(request);
        if (!IsValid(normalized))
            return new AchievementManagementResult(AchievementManagementStatus.InvalidAchievement);
        if (await TitleExistsAsync(courseId, year, normalized.Title, null, cancellationToken))
            return new AchievementManagementResult(AchievementManagementStatus.DuplicateTitle);

        var entity = new AchievementEntity
        {
            Id = Guid.NewGuid(),
            CourseID = courseId,
            Year = year,
            Title = normalized.Title,
            Description = normalized.Description ?? string.Empty,
            Rarity = normalized.Rarity,
            Track = normalized.Track ?? "default",
            LabID = normalized.LabId
        };
        dbContext.Achievements.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Achievement created. UserId={UserId}, Role={Role}, CourseId={CourseId}, Year={Year}, AchievementId={AchievementId}",
            userId,
            role,
            courseId,
            year,
            entity.Id);

        return new AchievementManagementResult(
            AchievementManagementStatus.Success,
            Achievement: ToDto(entity, 0, false));
    }

    public async Task<AchievementManagementResult> UpdateAsync(
        Guid userId,
        UserRole role,
        Guid courseId,
        int year,
        Guid achievementId,
        SaveAchievementRequest request,
        CancellationToken cancellationToken = default)
    {
        var accessStatus = await GetAccessStatusAsync(
            userId,
            role,
            courseId,
            year,
            cancellationToken);
        if (accessStatus != AchievementManagementStatus.Success)
            return new AchievementManagementResult(accessStatus);

        var entity = await FindAchievementAsync(courseId, year, achievementId, cancellationToken);
        if (entity is null)
            return new AchievementManagementResult(AchievementManagementStatus.AchievementNotFound);

        var normalized = Normalize(request);
        if (!IsValid(normalized))
            return new AchievementManagementResult(AchievementManagementStatus.InvalidAchievement);
        if (await TitleExistsAsync(
                courseId,
                year,
                normalized.Title,
                achievementId,
                cancellationToken))
        {
            return new AchievementManagementResult(AchievementManagementStatus.DuplicateTitle);
        }

        entity.Title = normalized.Title;
        entity.Description = normalized.Description ?? string.Empty;
        entity.Rarity = normalized.Rarity;
        entity.Track = normalized.Track ?? "default";
        entity.LabID = normalized.LabId;
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Achievement updated. UserId={UserId}, Role={Role}, CourseId={CourseId}, Year={Year}, AchievementId={AchievementId}",
            userId,
            role,
            courseId,
            year,
            entity.Id);

        return new AchievementManagementResult(
            AchievementManagementStatus.Success,
            Achievement: await BuildDtoAsync(entity, cancellationToken));
    }

    public async Task<AchievementManagementResult> DeleteAsync(
        Guid userId,
        UserRole role,
        Guid courseId,
        int year,
        Guid achievementId,
        bool revokeAwards,
        CancellationToken cancellationToken = default)
    {
        var accessStatus = await GetAccessStatusAsync(
            userId,
            role,
            courseId,
            year,
            cancellationToken);
        if (accessStatus != AchievementManagementStatus.Success)
            return new AchievementManagementResult(accessStatus);

        var entity = await FindAchievementAsync(courseId, year, achievementId, cancellationToken);
        if (entity is null)
            return new AchievementManagementResult(AchievementManagementStatus.AchievementNotFound);

        var awardCount = await dbContext.StudentAchievements.CountAsync(
                item => item.AchievementID == achievementId,
                cancellationToken);
        if (awardCount > 0 && !revokeAwards)
        {
            return new AchievementManagementResult(
                AchievementManagementStatus.AwardsConfirmationRequired);
        }

        if (await dbContext.AchievementConnections.AnyAsync(
                connection =>
                    connection.SourceId == achievementId || connection.TargetId == achievementId,
                cancellationToken))
        {
            return new AchievementManagementResult(AchievementManagementStatus.HasDependencies);
        }

        if (awardCount > 0)
        {
            var awards = await dbContext.StudentAchievements
                .Where(item => item.AchievementID == achievementId)
                .ToListAsync(cancellationToken);
            dbContext.StudentAchievements.RemoveRange(awards);
        }

        dbContext.Achievements.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Achievement deleted. UserId={UserId}, Role={Role}, CourseId={CourseId}, Year={Year}, AchievementId={AchievementId}, RevokedAwardCount={RevokedAwardCount}",
            userId,
            role,
            courseId,
            year,
            entity.Id,
            awardCount);

        return new AchievementManagementResult(AchievementManagementStatus.Success);
    }

    public async Task<AchievementManagementResult> GetAwardsAsync(
        Guid userId,
        UserRole role,
        Guid courseId,
        int year,
        Guid achievementId,
        CancellationToken cancellationToken = default)
    {
        var accessStatus = await GetAccessStatusAsync(
            userId,
            role,
            courseId,
            year,
            cancellationToken);
        if (accessStatus != AchievementManagementStatus.Success)
            return new AchievementManagementResult(accessStatus);

        if (await FindAchievementAsync(courseId, year, achievementId, cancellationToken) is null)
            return new AchievementManagementResult(AchievementManagementStatus.AchievementNotFound);

        var awards = await dbContext.StudentAchievements
            .AsNoTracking()
            .Where(item => item.AchievementID == achievementId)
            .OrderBy(item => item.StudentID)
            .Select(item => new ManagedAchievementAwardDto(item.StudentID))
            .ToListAsync(cancellationToken);

        return new AchievementManagementResult(
            AchievementManagementStatus.Success,
            Awards: awards);
    }

    public async Task<AchievementManagementResult> RevokeAwardAsync(
        Guid userId,
        UserRole role,
        Guid courseId,
        int year,
        Guid achievementId,
        Guid studentId,
        CancellationToken cancellationToken = default)
    {
        var accessStatus = await GetAccessStatusAsync(
            userId,
            role,
            courseId,
            year,
            cancellationToken);
        if (accessStatus != AchievementManagementStatus.Success)
            return new AchievementManagementResult(accessStatus);

        var achievement = await FindAchievementAsync(
            courseId,
            year,
            achievementId,
            cancellationToken);
        if (achievement is null)
            return new AchievementManagementResult(AchievementManagementStatus.AchievementNotFound);

        var award = await dbContext.StudentAchievements.SingleOrDefaultAsync(
            item =>
                item.AchievementID == achievementId &&
                item.StudentID == studentId,
            cancellationToken);
        if (award is null)
            return new AchievementManagementResult(AchievementManagementStatus.AwardNotFound);

        dbContext.StudentAchievements.Remove(award);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Achievement award revoked. UserId={UserId}, Role={Role}, CourseId={CourseId}, Year={Year}, AchievementId={AchievementId}, StudentId={StudentId}",
            userId,
            role,
            courseId,
            year,
            achievementId,
            studentId);

        return new AchievementManagementResult(
            AchievementManagementStatus.Success,
            Achievement: await BuildDtoAsync(achievement, cancellationToken));
    }

    public async Task<AchievementManagementResult> SaveCriteriaAsync(
        Guid userId,
        UserRole role,
        Guid courseId,
        int year,
        Guid achievementId,
        SaveAchievementCriteriaRequest request,
        CancellationToken cancellationToken = default)
    {
        var accessStatus = await GetAccessStatusAsync(
            userId,
            role,
            courseId,
            year,
            cancellationToken);
        if (accessStatus != AchievementManagementStatus.Success)
            return new AchievementManagementResult(accessStatus);

        var entity = await FindAchievementAsync(courseId, year, achievementId, cancellationToken);
        if (entity is null)
            return new AchievementManagementResult(AchievementManagementStatus.AchievementNotFound);

        var expression = request.Expression?.Trim() ?? string.Empty;
        var tags = AchievementTagMatcher.ParseExpression(expression);
        if (expression.Length > 1000 || tags.Count == 0 || tags.Any(tag => tag.Length > 100))
            return new AchievementManagementResult(AchievementManagementStatus.InvalidCriteria);

        if (entity.Criteria is null)
        {
            var criteria = new AchievementCriteriaEntity
            {
                Id = Guid.NewGuid(),
                AchievementID = entity.Id,
                Achievement = entity
            };
            entity.Criteria = criteria;
            dbContext.AchievementCriterias.Add(criteria);
        }

        entity.Criteria.Expression = string.Join(", ", tags);
        entity.Criteria.Scope = request.Scope;
        entity.Criteria.IsEnabled = request.IsEnabled;
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Achievement criteria saved. UserId={UserId}, Role={Role}, CourseId={CourseId}, Year={Year}, AchievementId={AchievementId}, CriteriaId={CriteriaId}",
            userId,
            role,
            courseId,
            year,
            entity.Id,
            entity.Criteria.Id);

        return new AchievementManagementResult(
            AchievementManagementStatus.Success,
            Achievement: await BuildDtoAsync(entity, cancellationToken));
    }

    public async Task<AchievementManagementResult> DeleteCriteriaAsync(
        Guid userId,
        UserRole role,
        Guid courseId,
        int year,
        Guid achievementId,
        CancellationToken cancellationToken = default)
    {
        var accessStatus = await GetAccessStatusAsync(
            userId,
            role,
            courseId,
            year,
            cancellationToken);
        if (accessStatus != AchievementManagementStatus.Success)
            return new AchievementManagementResult(accessStatus);

        var entity = await FindAchievementAsync(courseId, year, achievementId, cancellationToken);
        if (entity is null)
            return new AchievementManagementResult(AchievementManagementStatus.AchievementNotFound);
        if (entity.Criteria is null)
            return new AchievementManagementResult(AchievementManagementStatus.CriteriaNotFound);

        dbContext.AchievementCriterias.Remove(entity.Criteria);
        entity.Criteria = null;
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Achievement criteria deleted. UserId={UserId}, Role={Role}, CourseId={CourseId}, Year={Year}, AchievementId={AchievementId}",
            userId,
            role,
            courseId,
            year,
            entity.Id);

        return new AchievementManagementResult(
            AchievementManagementStatus.Success,
            Achievement: await BuildDtoAsync(entity, cancellationToken));
    }

    private async Task<AchievementManagementStatus> GetAccessStatusAsync(
        Guid userId,
        UserRole role,
        Guid courseId,
        int year,
        CancellationToken cancellationToken)
    {
        if (!accessPolicy.Can(role, Permission.EditAchievementCriteria))
            return AchievementManagementStatus.AccessDenied;

        var courseResult = await staffCourseService.GetCourseAsync(
            userId,
            role,
            courseId,
            year,
            cancellationToken);
        return courseResult.Status switch
        {
            StaffCourseQueryStatus.Success => AchievementManagementStatus.Success,
            StaffCourseQueryStatus.CourseNotFound => AchievementManagementStatus.CourseNotFound,
            _ => AchievementManagementStatus.AccessDenied
        };
    }

    private Task<AchievementEntity?> FindAchievementAsync(
        Guid courseId,
        int year,
        Guid achievementId,
        CancellationToken cancellationToken)
    {
        return dbContext.Achievements
            .Include(achievement => achievement.Criteria)
            .SingleOrDefaultAsync(
                achievement =>
                    achievement.Id == achievementId &&
                    achievement.CourseID == courseId &&
                    achievement.Year == year,
                cancellationToken);
    }

    private Task<bool> TitleExistsAsync(
        Guid courseId,
        int year,
        string title,
        Guid? excludedAchievementId,
        CancellationToken cancellationToken)
    {
        return dbContext.Achievements.AnyAsync(
            achievement =>
                achievement.CourseID == courseId &&
                achievement.Year == year &&
                achievement.Title == title &&
                (!excludedAchievementId.HasValue || achievement.Id != excludedAchievementId.Value),
            cancellationToken);
    }

    private async Task<IReadOnlyList<ManagedAchievementDto>> LoadDtosAsync(
        Guid courseId,
        int year,
        CancellationToken cancellationToken)
    {
        var entities = await dbContext.Achievements
            .AsNoTracking()
            .Include(achievement => achievement.Criteria)
            .Where(achievement => achievement.CourseID == courseId && achievement.Year == year)
            .OrderBy(achievement => achievement.Title)
            .ToListAsync(cancellationToken);
        var ids = entities.Select(achievement => achievement.Id).ToList();
        var awardCounts = (await dbContext.StudentAchievements
                .AsNoTracking()
                .Where(item => ids.Contains(item.AchievementID))
                .GroupBy(item => item.AchievementID)
                .Select(group => new { AchievementId = group.Key, Count = group.Count() })
                .ToListAsync(cancellationToken))
            .ToDictionary(item => item.AchievementId, item => item.Count);
        var connections = await dbContext.AchievementConnections
            .AsNoTracking()
            .Where(connection =>
                ids.Contains(connection.SourceId) || ids.Contains(connection.TargetId))
            .Select(connection => new { connection.SourceId, connection.TargetId })
            .ToListAsync(cancellationToken);
        var connectedIds = connections
            .SelectMany(connection => new[] { connection.SourceId, connection.TargetId })
            .ToHashSet();

        return entities
            .Select(entity => ToDto(
                entity,
                awardCounts.GetValueOrDefault(entity.Id),
                connectedIds.Contains(entity.Id)))
            .ToList();
    }

    private async Task<ManagedAchievementDto> BuildDtoAsync(
        AchievementEntity entity,
        CancellationToken cancellationToken)
    {
        var awardCount = await dbContext.StudentAchievements
            .AsNoTracking()
            .CountAsync(item => item.AchievementID == entity.Id, cancellationToken);
        var hasDependencies = await dbContext.AchievementConnections
            .AsNoTracking()
            .AnyAsync(
                connection =>
                    connection.SourceId == entity.Id || connection.TargetId == entity.Id,
                cancellationToken);
        return ToDto(entity, awardCount, hasDependencies);
    }

    private static ManagedAchievementDto ToDto(
        AchievementEntity entity,
        int awardCount,
        bool hasDependencies)
    {
        return new ManagedAchievementDto(
            entity.Id,
            entity.Title,
            entity.Description,
            entity.Rarity,
            entity.Track,
            entity.LabID,
            entity.CourseID,
            entity.Year,
            awardCount > 0,
            awardCount,
            hasDependencies,
            entity.Criteria is null
                ? null
                : new ManagedAchievementCriteriaDto(
                    entity.Criteria.Id,
                    entity.Criteria.Expression,
                    entity.Criteria.Scope,
                    entity.Criteria.IsEnabled));
    }

    private static SaveAchievementRequest Normalize(SaveAchievementRequest request)
    {
        return request with
        {
            Title = request.Title?.Trim() ?? string.Empty,
            Description = request.Description?.Trim() ?? string.Empty,
            Track = string.IsNullOrWhiteSpace(request.Track) ? "default" : request.Track.Trim()
        };
    }

    private static bool IsValid(SaveAchievementRequest request)
    {
        return request.Title.Length is > 0 and <= 200 &&
               (request.Description?.Length ?? 0) <= 2000 &&
               request.Track is { Length: > 0 and <= 100 };
    }
}
