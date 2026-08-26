namespace Platform.DataAccess.Postgress
{
    public class AchievementEntity
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Year { get; set; }
        public AchievementRarity Rarity { get; set; } = AchievementRarity.Common;
        public string Track { get; set; } = "default";
        public Guid? LabID { get; set; }
        public Guid CourseID { get; set; }
        public List<StudentAchievementEntity> StudentAchievements { get; set; } = [];
        public AchievementCriteriaEntity? Criteria { get; set; }
    }
}
