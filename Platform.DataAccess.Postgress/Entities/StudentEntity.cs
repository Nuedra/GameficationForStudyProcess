namespace Platform.DataAccess.Postgress
{
    public class StudentEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;

        public List<StudentAchievementEntity> StudentAchievements { get; set; } = [];
        public List<GroupStudentEntity> GroupMemberships { get; set; } = [];
        public List<CourseInstanceStudentEntity> CourseEnrollments { get; set; } = [];
    }
}
