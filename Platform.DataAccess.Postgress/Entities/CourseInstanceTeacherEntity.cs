namespace Platform.DataAccess.Postgress;

/// <summary>
/// Временная локальная проекция назначения преподавателя на запуск курса.
/// Сущность принадлежит контуру LMS и не входит в AchievementDbContext.
/// </summary>
public sealed class CourseInstanceTeacherEntity
{
    public Guid CourseID { get; set; }
    public int Year { get; set; }
    public CourseInstanceEntity CourseInstance { get; set; } = null!;

    public Guid PersonID { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsLead { get; set; }
}
