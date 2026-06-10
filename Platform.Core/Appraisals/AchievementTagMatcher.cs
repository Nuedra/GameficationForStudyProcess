namespace Platform.Core.Appraisals;

public static class AchievementTagMatcher
{
    public static IReadOnlySet<string> ParseExpression(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return new HashSet<string>(StringComparer.Ordinal);

        return expression
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet(StringComparer.Ordinal);
    }

    public static bool IsMatch(string expression, IEnumerable<string> completedTags)
    {
        ArgumentNullException.ThrowIfNull(completedTags);

        var requiredTags = ParseExpression(expression);
        if (requiredTags.Count == 0)
            return false;

        var completedTagSet = completedTags.ToHashSet(StringComparer.Ordinal);
        return requiredTags.All(completedTagSet.Contains);
    }
}
