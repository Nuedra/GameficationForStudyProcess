namespace Platform.Application.Contracts;

public sealed record TeacherOptionDto(
    Guid Id,
    string DisplayName);

public sealed record TeachingAssignmentDto(
    Guid TeacherId,
    string TeacherName,
    DateTimeOffset StartDate,
    DateTimeOffset? EndDate,
    bool IsLead,
    bool IsActive);

public sealed record TeachingAssignmentManagementDto(
    Guid CourseId,
    int Year,
    IReadOnlyList<TeacherOptionDto> AvailableTeachers,
    IReadOnlyList<TeachingAssignmentDto> Assignments);

public sealed record SaveTeachingAssignmentRequest(
    DateTimeOffset StartDate,
    DateTimeOffset? EndDate,
    bool IsLead);
