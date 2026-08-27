using Platform.Application.Contracts;
using Platform.Core.Models;

namespace Platform.Application.Services;

public interface IAchievementManagementService
{
    Task<AchievementManagementResult> GetAllAsync(
        Guid userId,
        UserRole role,
        Guid courseId,
        int year,
        CancellationToken cancellationToken = default);

    Task<AchievementManagementResult> CreateAsync(
        Guid userId,
        UserRole role,
        Guid courseId,
        int year,
        SaveAchievementRequest request,
        CancellationToken cancellationToken = default);

    Task<AchievementManagementResult> UpdateAsync(
        Guid userId,
        UserRole role,
        Guid courseId,
        int year,
        Guid achievementId,
        SaveAchievementRequest request,
        CancellationToken cancellationToken = default);

    Task<AchievementManagementResult> DeleteAsync(
        Guid userId,
        UserRole role,
        Guid courseId,
        int year,
        Guid achievementId,
        bool revokeAwards,
        CancellationToken cancellationToken = default);

    Task<AchievementManagementResult> GetAwardsAsync(
        Guid userId,
        UserRole role,
        Guid courseId,
        int year,
        Guid achievementId,
        CancellationToken cancellationToken = default);

    Task<AchievementManagementResult> RevokeAwardAsync(
        Guid userId,
        UserRole role,
        Guid courseId,
        int year,
        Guid achievementId,
        Guid studentId,
        CancellationToken cancellationToken = default);

    Task<AchievementManagementResult> GetAwardAuditAsync(
        Guid userId,
        UserRole role,
        Guid courseId,
        int year,
        Guid? achievementId = null,
        Guid? studentId = null,
        int limit = 100,
        CancellationToken cancellationToken = default);

    Task<AchievementManagementResult> SaveCriteriaAsync(
        Guid userId,
        UserRole role,
        Guid courseId,
        int year,
        Guid achievementId,
        SaveAchievementCriteriaRequest request,
        CancellationToken cancellationToken = default);

    Task<AchievementManagementResult> DeleteCriteriaAsync(
        Guid userId,
        UserRole role,
        Guid courseId,
        int year,
        Guid achievementId,
        CancellationToken cancellationToken = default);
}

public enum AchievementManagementStatus
{
    Success,
    AccessDenied,
    CourseNotFound,
    AchievementNotFound,
    CriteriaNotFound,
    AwardNotFound,
    InvalidAchievement,
    InvalidCriteria,
    DuplicateTitle,
    AwardsConfirmationRequired,
    HasDependencies
}

public sealed record AchievementManagementResult(
    AchievementManagementStatus Status,
    IReadOnlyList<ManagedAchievementDto>? Achievements = null,
    ManagedAchievementDto? Achievement = null,
    IReadOnlyList<ManagedAchievementAwardDto>? Awards = null,
    IReadOnlyList<AchievementAwardAuditEventDto>? AuditEvents = null);
