using Platform.DataAccess.Postgress;

namespace Platform.Application.Contracts;

public sealed record StudentStatisticsDto(
    int TotalAchievements,
    IReadOnlyList<AchievementRarityCountDto> ByRarity);

public sealed record AchievementRarityCountDto(
    AchievementRarity Rarity,
    int Count);
