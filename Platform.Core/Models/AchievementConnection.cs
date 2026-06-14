namespace Platform.Core.Models;
public class AchievementConnection
{
    private AchievementConnection(Guid id, Guid sourceId, Achievement source, Guid targetId, Achievement target)
    {
        Id = id;
        SourceId = sourceId;
        Source = source;
        TargetId = targetId;
        Target = target;
    }
    public Guid Id { get; }  
    public Guid SourceId { get; }
    public Achievement Source { get;  }
    public Guid TargetId { get;  }
    public Achievement Target { get; } 
}
