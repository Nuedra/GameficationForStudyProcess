namespace Platform.Application.Contracts;

public sealed record ApiErrorDto(
    string Code,
    string Message);

public static class ApiErrors
{
    public static readonly ApiErrorDto InvalidStudentId =
        new("invalid_student_id", "Student ID must be a non-empty GUID.");

    public static readonly ApiErrorDto InvalidCredentials =
        new("invalid_credentials", "Student with the specified ID was not found.");

    public static readonly ApiErrorDto AuthenticationRequired =
        new("authentication_required", "Student authentication is required.");

    public static readonly ApiErrorDto CourseNotFound =
        new("course_not_found", "The requested course instance was not found.");

    public static readonly ApiErrorDto CourseAccessDenied =
        new("course_access_denied", "The student is not enrolled in this course instance.");

    public static readonly ApiErrorDto AchievementGraphTemplateNotFound =
        new("achievement_graph_template_not_found", "Achievement graph XML template was not found.");

    public static readonly ApiErrorDto AchievementGraphTemplateInvalid =
        new("achievement_graph_template_invalid", "Achievement graph XML template is invalid.");
}
