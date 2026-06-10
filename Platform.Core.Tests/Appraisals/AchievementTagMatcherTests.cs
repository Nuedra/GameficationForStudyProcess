using Platform.Core.Appraisals;

namespace Platform.Core.Tests.Appraisals;

public sealed class AchievementTagMatcherTests
{
    [Theory]
    [InlineData("tag1", new[] { "tag1" })]
    [InlineData("tag1, tag2", new[] { "tag1", "tag2" })]
    [InlineData(" tag1 ,  tag2 ", new[] { "tag1", "tag2", "tag3" })]
    public void IsMatch_AllRequiredTagsAreCompleted_ReturnsTrue(
        string expression,
        string[] completedTags)
    {
        Assert.True(AchievementTagMatcher.IsMatch(expression, completedTags));
    }

    [Theory]
    [InlineData("tag1, tag2", new[] { "tag3" })]
    [InlineData("tag1, tag2", new[] { "tag1" })]
    [InlineData("tag1, tag2", new[] { "tag2" })]
    [InlineData("tag1, tag2", new[] { "TAG1", "TAG2" })]
    [InlineData("", new[] { "tag1" })]
    [InlineData(" , ", new[] { "tag1" })]
    public void IsMatch_NotAllRequiredTagsAreCompleted_ReturnsFalse(
        string expression,
        string[] completedTags)
    {
        Assert.False(AchievementTagMatcher.IsMatch(expression, completedTags));
    }
}
