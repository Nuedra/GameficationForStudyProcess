using Microsoft.EntityFrameworkCore;
using Platform.Application.Contracts;
using Platform.Core.Abstractions;
using Platform.Core.Appraisals;
using Platform.Core.Models;
using Platform.Core.Processing;
using Platform.DataAccess.Postgress;
using Platform.Lms;

namespace Platform.Application.Services;

public sealed class AchievementManagementService(
    AchievementDbContext dbContext,
    IStaffCourseService staffCourseService,
    IAccessPolicyService accessPolicy,
    ILmsDataSource lmsDataSource,
    AchievementProcessingCycle achievementProcessingCycle,
    TimeProvider timeProvider,
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
            var occurredAt = timeProvider.GetUtcNow().UtcDateTime;
            dbContext.AchievementAwardAuditEvents.AddRange(awards.Select(award =>
                CreateRevocationAuditEvent(
                    entity,
                    award,
                    userId,
                    role,
                    AchievementAwardAuditReason.AchievementDeletion,
                    occurredAt)));
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

    public async Task<AchievementManagementResult> GrantAwardAsync(
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

        var effectiveAt = timeProvider.GetUtcNow();
        var studentExists = await lmsDataSource.GetPersonAsync(
            studentId,
            cancellationToken) is not null;
        if (!studentExists)
        {
            await AddRejectedGrantAuditEventAsync(
                achievement,
                studentId,
                userId,
                role,
                AchievementAwardAuditReason.ManualGrantStudentNotFound,
                effectiveAt.UtcDateTime,
                cancellationToken);
            return new AchievementManagementResult(AchievementManagementStatus.StudentNotFound);
        }

        var hasActiveEnrollment = await lmsDataSource.HasActiveEnrollmentAsync(
            studentId,
            courseId,
            year,
            effectiveAt,
            cancellationToken);
        if (!hasActiveEnrollment)
        {
            await AddRejectedGrantAuditEventAsync(
                achievement,
                studentId,
                userId,
                role,
                AchievementAwardAuditReason.ManualGrantEnrollmentMissing,
                effectiveAt.UtcDateTime,
                cancellationToken);
            return new AchievementManagementResult(
                AchievementManagementStatus.StudentCourseEnrollmentRequired);
        }

        var alreadyAwarded = await dbContext.StudentAchievements
            .AsNoTracking()
            .AnyAsync(
                item =>
                    item.StudentID == studentId &&
                    item.AchievementID == achievementId,
                cancellationToken);
        if (alreadyAwarded)
        {
            await AddRejectedGrantAuditEventAsync(
                achievement,
                studentId,
                userId,
                role,
                AchievementAwardAuditReason.ManualGrantAlreadyExists,
                effectiveAt.UtcDateTime,
                cancellationToken);
            return new AchievementManagementResult(AchievementManagementStatus.AwardAlreadyExists);
        }

        if (!await HasEarnedPrerequisiteAsync(
                studentId,
                courseId,
                year,
                achievementId,
                cancellationToken))
        {
            await AddRejectedGrantAuditEventAsync(
                achievement,
                studentId,
                userId,
                role,
                AchievementAwardAuditReason.ManualGrantPrerequisiteMissing,
                effectiveAt.UtcDateTime,
                cancellationToken);
            return new AchievementManagementResult(
                AchievementManagementStatus.AchievementPrerequisiteMissing);
        }

        var now = effectiveAt.UtcDateTime;
        var award = new StudentAchievementEntity
        {
            Id = Guid.NewGuid(),
            StudentID = studentId,
            AchievementID = achievement.Id,
            Achievement = achievement,
            LabID = achievement.LabID,
            AchievementGotDate = now,
            AchievementFoundDate = now,
            IsNotificationSeen = false,
            IsFirstAnimationShown = false
        };

        dbContext.StudentAchievements.Add(award);
        var grantAuditEvent = CreateGrantAuditEvent(
            achievement,
            award,
            userId,
            role,
            now);
        dbContext.AchievementAwardAuditEvents.Add(grantAuditEvent);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueAwardViolation(exception))
        {
            DetachPendingGrant(award, grantAuditEvent);
            await AddRejectedGrantAuditEventAsync(
                achievement,
                studentId,
                userId,
                role,
                AchievementAwardAuditReason.ManualGrantAlreadyExists,
                now,
                cancellationToken);
            return new AchievementManagementResult(AchievementManagementStatus.AwardAlreadyExists);
        }

        await achievementProcessingCycle.RunAsync(studentId, cancellationToken);

        logger.LogInformation(
            "Achievement award granted manually. UserId={UserId}, Role={Role}, CourseId={CourseId}, Year={Year}, AchievementId={AchievementId}, StudentId={StudentId}",
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

        var occurredAt = timeProvider.GetUtcNow().UtcDateTime;
        var cascadeAwards = await GetUnsupportedDependentAwardsAfterRevocationAsync(
            studentId,
            courseId,
            year,
            achievementId,
            cancellationToken);
        var auditEvents = new List<AchievementAwardAuditEventEntity>
        {
            CreateRevocationAuditEvent(
                achievement,
                award,
                userId,
                role,
                AchievementAwardAuditReason.ManualRevocation,
                occurredAt)
        };
        auditEvents.AddRange(cascadeAwards.Select(cascadeAward =>
            CreateRevocationAuditEvent(
                cascadeAward.Achievement,
                cascadeAward,
                userId,
                role,
                AchievementAwardAuditReason.PrerequisiteRevocation,
                occurredAt)));

        dbContext.AchievementAwardAuditEvents.AddRange(auditEvents);
        dbContext.StudentAchievements.Remove(award);
        if (cascadeAwards.Count > 0)
            dbContext.StudentAchievements.RemoveRange(cascadeAwards);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Achievement award revoked. UserId={UserId}, Role={Role}, CourseId={CourseId}, Year={Year}, AchievementId={AchievementId}, StudentId={StudentId}, CascadedAwardCount={CascadedAwardCount}",
            userId,
            role,
            courseId,
            year,
            achievementId,
            studentId,
            cascadeAwards.Count);

        return new AchievementManagementResult(
            AchievementManagementStatus.Success,
            Achievement: await BuildDtoAsync(achievement, cancellationToken));
    }

    private async Task<IReadOnlyList<StudentAchievementEntity>> GetUnsupportedDependentAwardsAfterRevocationAsync(
        Guid studentId,
        Guid courseId,
        int year,
        Guid revokedAchievementId,
        CancellationToken cancellationToken)
    {
        var courseAchievementIds = await dbContext.Achievements
            .AsNoTracking()
            .Where(achievement =>
                achievement.CourseID == courseId &&
                achievement.Year == year)
            .Select(achievement => achievement.Id)
            .ToListAsync(cancellationToken);
        var courseAchievementIdSet = courseAchievementIds.ToHashSet();

        var earnedAwards = await dbContext.StudentAchievements
            .Include(award => award.Achievement)
            .Where(award =>
                award.StudentID == studentId &&
                courseAchievementIdSet.Contains(award.AchievementID))
            .ToListAsync(cancellationToken);
        var awardsByAchievementId = earnedAwards
            .Where(award => award.AchievementID != revokedAchievementId)
            .ToDictionary(award => award.AchievementID);
        var remainingEarnedIds = awardsByAchievementId.Keys.ToHashSet();

        var dependenciesByTarget = (await dbContext.AchievementConnections
                .AsNoTracking()
                .Where(connection =>
                    courseAchievementIdSet.Contains(connection.SourceId) &&
                    courseAchievementIdSet.Contains(connection.TargetId))
                .Select(connection => new
                {
                    connection.SourceId,
                    connection.TargetId
                })
                .ToListAsync(cancellationToken))
            .GroupBy(connection => connection.TargetId)
            .ToDictionary(
                group => group.Key,
                group => group.Select(connection => connection.SourceId).ToHashSet());
        var revokedIds = new HashSet<Guid>();

        var changed = true;
        while (changed)
        {
            changed = false;
            foreach (var award in awardsByAchievementId.Values)
            {
                if (revokedIds.Contains(award.AchievementID) ||
                    !dependenciesByTarget.TryGetValue(award.AchievementID, out var prerequisites) ||
                    prerequisites.Count == 0 ||
                    prerequisites.Any(remainingEarnedIds.Contains))
                {
                    continue;
                }

                revokedIds.Add(award.AchievementID);
                remainingEarnedIds.Remove(award.AchievementID);
                changed = true;
            }
        }

        return awardsByAchievementId.Values
            .Where(award => revokedIds.Contains(award.AchievementID))
            .ToList();
    }

    private async Task AddRejectedGrantAuditEventAsync(
        AchievementEntity achievement,
        Guid studentId,
        Guid actorId,
        UserRole actorRole,
        AchievementAwardAuditReason reason,
        DateTime occurredAt,
        CancellationToken cancellationToken)
    {
        dbContext.AchievementAwardAuditEvents.Add(CreateRejectedGrantAuditEvent(
            achievement,
            studentId,
            actorId,
            actorRole,
            reason,
            occurredAt));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<AchievementManagementResult> GetAwardAuditAsync(
        Guid userId,
        UserRole role,
        Guid courseId,
        int year,
        Guid? achievementId = null,
        Guid? studentId = null,
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        if (!accessPolicy.Can(role, Permission.ViewAchievementAudit))
            return new AchievementManagementResult(AchievementManagementStatus.AccessDenied);

        var accessStatus = await GetAccessStatusAsync(
            userId,
            role,
            courseId,
            year,
            cancellationToken);
        if (accessStatus != AchievementManagementStatus.Success)
            return new AchievementManagementResult(accessStatus);

        var normalizedLimit = Math.Clamp(limit, 1, 500);
        var query = dbContext.AchievementAwardAuditEvents
            .AsNoTracking()
            .Where(auditEvent =>
                auditEvent.CourseID == courseId &&
                auditEvent.Year == year);

        if (achievementId.HasValue)
            query = query.Where(auditEvent => auditEvent.AchievementID == achievementId.Value);
        if (studentId.HasValue)
            query = query.Where(auditEvent => auditEvent.StudentID == studentId.Value);

        var events = await query
            .OrderByDescending(auditEvent => auditEvent.OccurredAt)
            .ThenByDescending(auditEvent => auditEvent.Id)
            .Take(normalizedLimit)
            .Select(auditEvent => new AchievementAwardAuditEventDto(
                auditEvent.Id,
                auditEvent.AwardID,
                auditEvent.EventType,
                auditEvent.OccurredAt,
                auditEvent.AwardedAt,
                auditEvent.StudentID,
                auditEvent.AchievementID,
                auditEvent.AchievementTitle,
                auditEvent.CourseID,
                auditEvent.Year,
                auditEvent.ActorID,
                auditEvent.ActorRole,
                auditEvent.Reason,
                auditEvent.CriterionExpression,
                auditEvent.CriterionScope))
            .ToListAsync(cancellationToken);

        return new AchievementManagementResult(
            AchievementManagementStatus.Success,
            AuditEvents: events);
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

    private async Task<bool> HasEarnedPrerequisiteAsync(
        Guid studentId,
        Guid courseId,
        int year,
        Guid achievementId,
        CancellationToken cancellationToken)
    {
        var prerequisiteIds = await dbContext.AchievementConnections
            .AsNoTracking()
            .Where(connection =>
                connection.TargetId == achievementId &&
                connection.Source.CourseID == courseId &&
                connection.Source.Year == year)
            .Select(connection => connection.SourceId)
            .ToListAsync(cancellationToken);

        return prerequisiteIds.Count == 0 ||
            await dbContext.StudentAchievements
                .AsNoTracking()
                .AnyAsync(
                    item =>
                        item.StudentID == studentId &&
                        prerequisiteIds.Contains(item.AchievementID),
                    cancellationToken);
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

    private static AchievementAwardAuditEventEntity CreateGrantAuditEvent(
        AchievementEntity achievement,
        StudentAchievementEntity award,
        Guid actorId,
        UserRole actorRole,
        DateTime occurredAt)
    {
        return new AchievementAwardAuditEventEntity
        {
            Id = Guid.NewGuid(),
            AwardID = award.Id,
            EventType = AchievementAwardAuditEventType.Granted,
            OccurredAt = occurredAt,
            AwardedAt = award.AchievementGotDate,
            StudentID = award.StudentID,
            AchievementID = achievement.Id,
            AchievementTitle = achievement.Title,
            CourseID = achievement.CourseID,
            Year = achievement.Year,
            ActorID = actorId,
            ActorRole = ToAuditActorRole(actorRole, "grant"),
            Reason = AchievementAwardAuditReason.ManualGrant,
            CriterionExpression = achievement.Criteria?.Expression,
            CriterionScope = achievement.Criteria?.Scope
        };
    }

    private static AchievementAwardAuditEventEntity CreateRejectedGrantAuditEvent(
        AchievementEntity achievement,
        Guid studentId,
        Guid actorId,
        UserRole actorRole,
        AchievementAwardAuditReason reason,
        DateTime occurredAt)
    {
        return new AchievementAwardAuditEventEntity
        {
            Id = Guid.NewGuid(),
            AwardID = null,
            EventType = AchievementAwardAuditEventType.Rejected,
            OccurredAt = occurredAt,
            AwardedAt = null,
            StudentID = studentId,
            AchievementID = achievement.Id,
            AchievementTitle = achievement.Title,
            CourseID = achievement.CourseID,
            Year = achievement.Year,
            ActorID = actorId,
            ActorRole = ToAuditActorRole(actorRole, "grant"),
            Reason = reason,
            CriterionExpression = achievement.Criteria?.Expression,
            CriterionScope = achievement.Criteria?.Scope
        };
    }

    private static AchievementAwardAuditEventEntity CreateRevocationAuditEvent(
        AchievementEntity achievement,
        StudentAchievementEntity award,
        Guid actorId,
        UserRole actorRole,
        AchievementAwardAuditReason reason,
        DateTime occurredAt)
    {
        return new AchievementAwardAuditEventEntity
        {
            Id = Guid.NewGuid(),
            AwardID = award.Id,
            EventType = AchievementAwardAuditEventType.Revoked,
            OccurredAt = occurredAt,
            AwardedAt = award.AchievementGotDate,
            StudentID = award.StudentID,
            AchievementID = achievement.Id,
            AchievementTitle = achievement.Title,
            CourseID = achievement.CourseID,
            Year = achievement.Year,
            ActorID = actorId,
            ActorRole = ToAuditActorRole(actorRole, "revoke"),
            Reason = reason,
            CriterionExpression = achievement.Criteria?.Expression,
            CriterionScope = achievement.Criteria?.Scope
        };
    }

    private static AchievementAwardAuditActorRole ToAuditActorRole(
        UserRole role,
        string action)
    {
        return role switch
        {
            UserRole.Teacher => AchievementAwardAuditActorRole.Teacher,
            UserRole.Administrator => AchievementAwardAuditActorRole.Administrator,
            _ => throw new InvalidOperationException(
                $"Only staff can {action} achievement awards.")
        };
    }

    private void DetachPendingGrant(
        StudentAchievementEntity award,
        AchievementAwardAuditEventEntity auditEvent)
    {
        dbContext.Entry(award).State = EntityState.Detached;
        dbContext.Entry(auditEvent).State = EntityState.Detached;
    }

    private static bool IsUniqueAwardViolation(DbUpdateException exception)
    {
        const string uniqueViolationSqlState = "23505";
        const string awardIndexName = "IX_student_achievements_StudentID_AchievementID";

        for (var current = exception.InnerException;
             current is not null;
             current = current.InnerException)
        {
            var exceptionType = current.GetType();
            if (exceptionType.FullName != "Npgsql.PostgresException")
                continue;

            var sqlState = exceptionType.GetProperty("SqlState")?.GetValue(current) as string;
            var constraintName = exceptionType.GetProperty("ConstraintName")?.GetValue(current) as string;
            return sqlState == uniqueViolationSqlState &&
                string.Equals(constraintName, awardIndexName, StringComparison.Ordinal);
        }

        return false;
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
