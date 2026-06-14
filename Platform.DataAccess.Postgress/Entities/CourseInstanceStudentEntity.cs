namespace Platform.DataAccess.Postgress;

public sealed class CourseInstanceStudentEntity
{
    public Guid CourseID { get; set; }
    public int Year { get; set; }
    public CourseInstanceEntity CourseInstance { get; set; } = null!;

    public Guid PersonID { get; set; }
    public StudentEntity Student { get; set; } = null!;

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
