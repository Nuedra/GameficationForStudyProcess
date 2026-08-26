namespace Platform.Lms;

/// <summary>
/// Проекция Person.Persons, дополненная текущей учебной группой для пользовательского интерфейса.
/// </summary>
public sealed record LmsPerson(
    Guid Id,
    string FirstName,
    string? MiddleName,
    string LastName,
    string? CurrentEducationalGroupName)
{
    public string DisplayName => string.Join(
        " ",
        new[] { LastName, FirstName, MiddleName }
            .Where(part => !string.IsNullOrWhiteSpace(part)));
}
