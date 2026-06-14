namespace Platform.DataAccess.Postgress
{
    public class AchievementConnectionEntity
    {
        public Guid Id { get; set; }  
        public Guid SourceId { get; set; }
        public AchievementEntity Source { get; set; }
        public Guid TargetId { get; set; }

        public AchievementEntity Target { get; set; } 
    }
}
