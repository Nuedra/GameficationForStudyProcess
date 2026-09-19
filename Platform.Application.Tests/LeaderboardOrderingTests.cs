using Platform.Application.Services;
using Platform.DataAccess.Postgress;

namespace Platform.Application.Tests;

public sealed class LeaderboardOrderingTests
{
    [Fact]
    public void Order_UsesTotalThenEachRarityThenRussianAlphabet()
    {
        var entries = new[]
        {
            Candidate("Яков", common: 5),
            Candidate("Легендарный", legendary: 1, common: 3),
            Candidate("Эпический", epic: 2, common: 2),
            Candidate("Редкий два", epic: 1, rare: 2, common: 1),
            Candidate("Редкий один", epic: 1, rare: 1, common: 2),
            Candidate("Борис", epic: 1, common: 3),
            Candidate("Анна", epic: 1, common: 3)
        };

        var orderedNames = LeaderboardOrdering.Order(
                entries,
                entry => entry.Counts.Values.Sum(),
                (entry, rarity) => entry.Counts.GetValueOrDefault(rarity),
                entry => entry.Name,
                entry => entry.Group,
                entry => entry.Id)
            .Select(entry => entry.Name);

        Assert.Equal(
            [
                "Яков",
                "Легендарный",
                "Эпический",
                "Редкий два",
                "Редкий один",
                "Анна",
                "Борис"
            ],
            orderedNames);
    }

    private static TestCandidate Candidate(
        string name,
        int legendary = 0,
        int epic = 0,
        int rare = 0,
        int common = 0) =>
        new(
            Guid.NewGuid(),
            name,
            "ИВТ-101",
            new Dictionary<AchievementRarity, int>
            {
                [AchievementRarity.Legendary] = legendary,
                [AchievementRarity.Epic] = epic,
                [AchievementRarity.Rare] = rare,
                [AchievementRarity.Common] = common
            });

    private sealed record TestCandidate(
        Guid Id,
        string Name,
        string? Group,
        IReadOnlyDictionary<AchievementRarity, int> Counts);
}
