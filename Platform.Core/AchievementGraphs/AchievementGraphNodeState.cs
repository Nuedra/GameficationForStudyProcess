namespace Platform.Core.AchievementGraphs;

public sealed record AchievementGraphNodeState(
    Guid AchievementId,
    string Title,
    AchievementGraphStatus Status,
    string? GraphNodeId = null);
