namespace Platform.DataAccess.Postgress;

public sealed class GroupStudentEntity
{
    public Guid PersonID { get; set; }
    public StudentEntity Student { get; set; } = null!;

    public string EdGroupID { get; set; } = string.Empty;
    public EducationalGroupEntity EducationalGroup { get; set; } = null!;

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
