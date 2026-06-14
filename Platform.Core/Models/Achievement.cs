namespace Platform.Core.Models

{
    public class Achievement
    {
        public Achievement(Platform.DataAccess.Postgress.AchievementEntity entity)
        : this(
            entity.Id,
            entity.Title,
            entity.Description,
            entity.Year,
            entity.Rarity,
            entity.Track,
            entity.LabID,
            entity.CourseID,
            entity.Course != null
                ? new Course(entity.Course)
                : null!,
            entity.StudentAchievements
                .Select(sa => new StudentAchievement(sa))
                .ToList(),
            entity.Criteria != null
                ? new AchievementCriteria(entity.Criteria)
                : null!
        )
        {
        }
        private Achievement(Guid id, string title, string description, int year, string rarity, string track, Guid? labID,
        Guid courseID, Course course, List<StudentAchievement> studentAchievements, AchievementCriteria criteria)
        {
            Id = id;
            Title = title;
            Description = description;
            Year = year;
            Rarity = rarity;
            Track = track;
            LabID = labID;
            CourseID = courseID;
            Course = course;
            StudentAchievements = studentAchievements;
            Criteria = criteria;
        }

        public Guid Id { get; }
        public string Title { get;} = string.Empty;
        public string Description { get; } = string.Empty;
        public int Year { get;}
        public string Rarity { get; } = "Common";
        public string Track { get; } = "default";
        public Guid? LabID { get; }
        public Guid CourseID { get; }
        public Course Course { get; }
        public List<StudentAchievement> StudentAchievements { get;} = [];
        public AchievementCriteria Criteria { get; }
    }
}
