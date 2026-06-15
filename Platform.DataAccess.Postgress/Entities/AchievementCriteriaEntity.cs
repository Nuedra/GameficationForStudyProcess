namespace Platform.DataAccess.Postgress
{
    public class AchievementCriteriaEntity
    {
        public Guid Id { get; set; }
        public bool IsEnabled { get; set; } = true;
        public string Expression { get; set; } = string.Empty;
        public AchievementCriteriaScope Scope { get; set; } = AchievementCriteriaScope.SameMark;
        public Guid AchievementID { get; set; }
        public AchievementEntity Achievement { get; set; }
    }
}
