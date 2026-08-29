namespace Platform.DataAccess.Postgress;

/// <summary>
/// Неизменяемое предметное событие выдачи или отзыва достижения.
/// Снимки названия и критерия намеренно хранятся без внешних ключей:
/// история должна переживать удаление достижения и изменения в LMS.
/// </summary>
public sealed class AchievementAwardAuditEventEntity
{
    public Guid Id { get; set; }
    public Guid? AwardID { get; set; }
    public AchievementAwardAuditEventType EventType { get; set; }
    public DateTime OccurredAt { get; set; }
    public DateTime? AwardedAt { get; set; }
    public Guid StudentID { get; set; }
    public Guid AchievementID { get; set; }
    public string AchievementTitle { get; set; } = string.Empty;
    public Guid CourseID { get; set; }
    public int Year { get; set; }
    public Guid? ActorID { get; set; }
    public AchievementAwardAuditActorRole ActorRole { get; set; }
    public AchievementAwardAuditReason Reason { get; set; }
    public string? CriterionExpression { get; set; }
    public AchievementCriteriaScope? CriterionScope { get; set; }
}

public enum AchievementAwardAuditEventType
{
    Granted,
    Revoked,
    Rejected
}

public enum AchievementAwardAuditActorRole
{
    System,
    Teacher,
    Administrator
}

public enum AchievementAwardAuditReason
{
    CriteriaMatched,
    ManualGrant,
    ManualRevocation,
    AchievementDeletion,
    PrerequisiteRevocation,
    ManualGrantStudentNotFound,
    ManualGrantEnrollmentMissing,
    ManualGrantAlreadyExists,
    ManualGrantPrerequisiteMissing
}
