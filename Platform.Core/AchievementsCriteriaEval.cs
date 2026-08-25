namespace Platform.Core.Evaluation
{
    /// <summary>
    /// Устаревший прототип evaluator. Рабочая проверка критериев выполняется
    /// в AchievementProcessingCycle по типизированным фактам ведомости.
    /// </summary>
    [Obsolete(
        "Используйте AchievementProcessingCycle. " +
        "Этот прототип не реализует вычисление критериев.")]
    public static class AchievementsCriteriaEvalEvaluator
    {
        public static bool Evaluate(string criteria, Dictionary<string, object?> data)
        {
            throw new NotSupportedException(
                "Устаревший evaluator отключён, поскольку он не реализует " +
                "проверку критериев достижений.");
        }
    }
}
