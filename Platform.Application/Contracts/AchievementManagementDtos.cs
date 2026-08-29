using Platform.DataAccess.Postgress;

namespace Platform.Application.Contracts;

public sealed record ManagedAchievementDto(
    Guid Id,
    string Title,
    string Description,
    AchievementRarity Rarity,
    string Track,
    Guid? LabId,
    Guid CourseId,
    int Year,
    bool HasAwards,
    int AwardCount,
    bool HasDependencies,
    ManagedAchievementCriteriaDto? Criteria);

public sealed record ManagedAchievementCriteriaDto(
    Guid Id,
    string Expression,
    AchievementCriteriaScope Scope,
    bool IsEnabled);

public sealed record ManagedAchievementAwardDto(
    Guid StudentId);

public sealed record ManualAchievementAwardRequest(
    Guid StudentId);

public sealed record AchievementAwardAuditEventDto(
    Guid Id,
    Guid? AwardId,
    AchievementAwardAuditEventType EventType,
    DateTime OccurredAt,
    DateTime? AwardedAt,
    Guid StudentId,
    Guid AchievementId,
    string AchievementTitle,
    Guid CourseId,
    int Year,
    Guid? ActorId,
    AchievementAwardAuditActorRole ActorRole,
    AchievementAwardAuditReason Reason,
    string? CriterionExpression,
    AchievementCriteriaScope? CriterionScope);

public sealed record SaveAchievementRequest(
    string Title,
    string? Description,
    AchievementRarity Rarity,
    string? Track,
    Guid? LabId);

public sealed record SaveAchievementCriteriaRequest(
    string Expression,
    AchievementCriteriaScope Scope,
    bool IsEnabled);
