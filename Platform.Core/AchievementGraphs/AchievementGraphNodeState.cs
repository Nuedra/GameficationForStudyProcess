namespace Platform.Core.AchievementGraphs;

public sealed record AchievementGraphNodeState(
    Guid AchievementId,
    AchievementGraphStatus Status);
