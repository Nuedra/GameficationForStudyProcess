namespace Platform.DataAccess.Postgress
{
    public class AchievementEntity
    {
        
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Rarity { get; set; } = "Common";
        public string Track { get; set; } = "default";
        public Guid CourseID { get; set; }
        public CourseEntity Course { get; set; }
        public List<StudentAchievementEntity> StudentAchievements { get; set; } = [];
        public AchievementCriteriaEntity Criteria { get; set; }
    }
}
