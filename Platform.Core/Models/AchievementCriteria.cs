namespace Platform.Core.Models;

public class AchievementCriteria
{
    public AchievementCriteria(Platform.DataAccess.Postgress.AchievementCriteriaEntity entity)
    : this(
        entity.Id,
        entity.IsEnabled,
        entity.Expression,
        entity.Scope,
        entity.AchievementID,
        entity.Achievement != null
            ? new Achievement(entity.Achievement)
            : null!
    )
    {
    }

    private AchievementCriteria(
        Guid id,
        bool isEnabled,
        string expression,
        Platform.DataAccess.Postgress.AchievementCriteriaScope scope,
        Guid achievementID,
        Achievement achievement)
    {
        Id = id;
        IsEnabled = isEnabled;
        Expression = expression;
        Scope = scope;
        AchievementID = achievementID;
        Achievement = achievement;
    }
    public Guid Id { get;}
    public bool IsEnabled { get; } = true;
    public string Expression { get; } = string.Empty;
    public Platform.DataAccess.Postgress.AchievementCriteriaScope Scope { get; }
    public Guid AchievementID { get; }
    public Achievement Achievement { get; }
}
