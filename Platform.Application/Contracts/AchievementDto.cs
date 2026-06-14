namespace Platform.Application.Contracts;

public enum AchievementStatus
{
    Locked,
    Available,
    Earned
}

public sealed record AchievementDto(
    Guid Id,
    string Name,
    string Description,
    AchievementStatus Status,
    DateTimeOffset? EarnedAt,
    string? ImageUrl);
