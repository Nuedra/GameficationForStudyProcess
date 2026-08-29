namespace Platform.Lms;

/// <summary>
/// Студент, активно обучающийся на конкретном экземпляре курса.
/// </summary>
public sealed record LmsCourseStudent(
    Guid Id,
    string FirstName,
    string? MiddleName,
    string LastName,
    string? CurrentEducationalGroupName)
{
    public string DisplayName => string.Join(
        " ",
        new[] { FirstName, LastName, MiddleName }
            .Where(part => !string.IsNullOrWhiteSpace(part)));
}
