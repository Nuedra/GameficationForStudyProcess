namespace Platform.Application.Contracts;

public sealed record ApiErrorDto(
    string Code,
    string Message);

public static class ApiErrors
{
    public static readonly ApiErrorDto InvalidStudentId =
        new("invalid_student_id", "Укажите корректный ID студента в формате GUID.");

    public static readonly ApiErrorDto InvalidCredentials =
        new("invalid_credentials", "Студент с таким ID не найден. Проверьте введённое значение.");

    public static readonly ApiErrorDto AuthenticationRequired =
        new("authentication_required", "Сессия отсутствует или завершилась. Выполните вход снова.");

    public static readonly ApiErrorDto CourseNotFound =
        new("course_not_found", "Курс за указанный учебный год не найден.");

    public static readonly ApiErrorDto CourseAccessDenied =
        new("course_access_denied", "У вас нет доступа к этому курсу.");

    public static readonly ApiErrorDto AchievementGraphTemplateNotFound =
        new("achievement_graph_template_not_found", "Шаблон графа достижений не найден. Обратитесь к администратору.");

    public static readonly ApiErrorDto AchievementGraphTemplateInvalid =
        new("achievement_graph_template_invalid", "Шаблон графа достижений содержит ошибку. Обратитесь к администратору.");

    public static readonly ApiErrorDto InternalServerError =
        new("internal_server_error", "Сервис временно недоступен. Повторите запрос позже.");
}
