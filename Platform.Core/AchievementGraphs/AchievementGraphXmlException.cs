namespace Platform.Core.AchievementGraphs;

public sealed class AchievementGraphXmlException : Exception
{
    public AchievementGraphXmlException(string message)
        : base(message)
    {
    }

    public AchievementGraphXmlException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
