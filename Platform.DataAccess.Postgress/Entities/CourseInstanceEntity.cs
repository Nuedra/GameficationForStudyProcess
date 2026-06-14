namespace Platform.DataAccess.Postgress;

public sealed class CourseInstanceEntity
{
    public Guid CourseID { get; set; }
    public CourseEntity Course { get; set; } = null!;

    public int Year { get; set; }
    public Guid ContentScopeID { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<CourseInstanceStudentEntity> Students { get; set; } = [];
}
