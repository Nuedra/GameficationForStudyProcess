namespace Platform.Application.Contracts;

public sealed record CourseDto(
    Guid Id,
    string Title,
    string Description,
    int Year);
