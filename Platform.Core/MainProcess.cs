namespace Platform.Core
{
    /// <summary>
    /// Устаревшая точка входа экспериментального цикла обработки достижений.
    /// Рабочий сценарий приложения реализован в <see cref="Processing.AchievementProcessingCycle"/>.
    /// </summary>
    [Obsolete(
        "Используйте AchievementProcessingCycle и IAppraisalPayloadProvider. " +
        "Старый сценарий не поддерживает контракт внешнего API.")]
    public static class Activities
    {
        /// <summary>
        /// Оставлен только для явной диагностики старых вызовов. Запуск запрещён,
        /// поскольку прежний evaluator безусловно подтверждал любой критерий.
        /// </summary>
        public static Task MainProcess(Guid studentId)
        {
            throw new NotSupportedException(
                "Устаревший MainProcess отключён. Используйте AchievementProcessingCycle " +
                "с реализацией IAppraisalPayloadProvider.");
        }
    }
}
