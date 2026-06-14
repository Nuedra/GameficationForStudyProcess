namespace Platform.DataAccess.Postgress
{
    public class StudentAchievementEntity
    {
        public Guid Id { get; set; }
        public DateTime AchievementGotDate { get; set;}
        public DateTime AchievementFoundDate { get; set;}   
        public bool IsNotificationSeen { get; set;} = false; 
        public bool IsFirstAnimationShown { get; set;} = false;
        public Guid? LabID { get; set; }
        public Guid AchievementID { get; set;}
        public AchievementEntity Achievement { get; set;}
        public Guid StudentID { get; set;}
        public StudentEntity Student { get; set;}

    }
}
