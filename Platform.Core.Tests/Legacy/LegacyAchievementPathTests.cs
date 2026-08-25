using Platform.Core.Evaluation;

namespace Platform.Core.Tests.Legacy;

public sealed class LegacyAchievementPathTests
{
    [Fact]
    public async Task MainProcess_AlwaysRejectsLegacyExecution()
    {
#pragma warning disable CS0618
        var exception = await Assert.ThrowsAsync<NotSupportedException>(
            () => Activities.MainProcess(Guid.NewGuid()));
#pragma warning restore CS0618

        Assert.Contains("AchievementProcessingCycle", exception.Message);
    }

    [Fact]
    public void CriteriaEvaluator_NeverApprovesAchievementByDefault()
    {
#pragma warning disable CS0618
        var exception = Assert.Throws<NotSupportedException>(() =>
            AchievementsCriteriaEvalEvaluator.Evaluate(
                "any_criteria",
                new Dictionary<string, object?>()));
#pragma warning restore CS0618

        Assert.Contains("не реализует", exception.Message);
    }
}
