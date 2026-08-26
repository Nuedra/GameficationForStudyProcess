namespace Platform.Lms;

/// <summary>
/// Проекция Course.Courses и Course.CourseInstances для подсистемы достижений.
/// </summary>
public sealed record LmsCourseInstance(
    Guid CourseId,
    int Year,
    string Name,
    Guid ContentScopeId,
    string? Description = null);
