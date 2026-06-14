namespace Platform.DataAccess.Postgress;

public sealed class EducationalGroupEntity
{
    public string GroupName { get; set; } = string.Empty;
    public string GroupCaption { get; set; } = string.Empty;
    public Guid EdProgramID { get; set; }
    public int AdmissionYear { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public List<GroupStudentEntity> Students { get; set; } = [];
}
