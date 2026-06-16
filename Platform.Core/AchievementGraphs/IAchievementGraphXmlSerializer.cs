namespace Platform.Core.AchievementGraphs;

public interface IAchievementGraphXmlSerializer
{
    string Serialize(
        string template,
        IReadOnlyCollection<AchievementGraphNodeState> nodeStates);
}
