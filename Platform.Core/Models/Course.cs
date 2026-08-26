namespace Platform.Core.Models;

public class Course
{
    private Course(Guid id, string title, string description, string authorEntity,
     List<Achievement> achievements, Guid? previousID)
    {
        Id = id;
        Title = title;
        Description = description;
        AuthorEntity = authorEntity;
        Achievements = achievements;
        PreviousID = previousID;
    }

    public Course(Platform.DataAccess.Postgress.CourseEntity entity)
    : this(
        entity.Id,
        entity.Title,
        entity.Description,
        entity.AuthorEntity,
        [],
        entity.PreviousID
    )
    {
    }

    public Guid Id { get; }
    public string Title { get; } = string.Empty;
    public string Description { get; } = string.Empty;
    public string AuthorEntity { get; } = string.Empty;
    public List<Achievement> Achievements { get; } = [];
    public Guid? PreviousID { get; }
    public Course? PreviousCourse { get; }

    public static (Course course, string error) Create(Guid id, string title, string description, 
    string authorEntity, List<Achievement> achievements, Guid? previousID)
    {
        var error = string.Empty;
        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description) || string.IsNullOrEmpty(authorEntity))
        {
            error = "Name/surname/group cannot be empty";
        }
        var course = new Course(id, title, description, authorEntity, achievements, previousID);
        return (course, error);
    }
}
