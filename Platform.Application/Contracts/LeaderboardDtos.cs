namespace Platform.Application.Contracts;

public sealed record LeaderboardEntryDto(
    Guid StudentId,
    string StudentName,
    string? Group,
    int AchievementCount);
