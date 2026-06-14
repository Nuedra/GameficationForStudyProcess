namespace Platform.Application.Contracts;

public sealed record StudentDto(
    Guid Id,
    string FullName,
    string Group);
