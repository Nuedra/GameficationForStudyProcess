using System.Globalization;
using Platform.DataAccess.Postgress;

namespace Platform.Application.Services;

internal static class LeaderboardOrdering
{
    private static readonly AchievementRarity[] RaritiesByPriority =
    [
        AchievementRarity.Legendary,
        AchievementRarity.Epic,
        AchievementRarity.Rare,
        AchievementRarity.Common
    ];

    private static readonly StringComparer StudentNameComparer = StringComparer.Create(
        CultureInfo.GetCultureInfo("ru-RU"),
        ignoreCase: true);

    public static IOrderedEnumerable<T> Order<T>(
        IEnumerable<T> entries,
        Func<T, int> achievementCount,
        Func<T, AchievementRarity, int> rarityCount,
        Func<T, string> studentName,
        Func<T, string?> group,
        Func<T, Guid> studentId)
    {
        var ordered = entries.OrderByDescending(achievementCount);

        foreach (var rarity in RaritiesByPriority)
        {
            ordered = ordered.ThenByDescending(entry => rarityCount(entry, rarity));
        }

        return ordered
            .ThenBy(studentName, StudentNameComparer)
            .ThenBy(group, StudentNameComparer)
            .ThenBy(studentId);
    }
}
