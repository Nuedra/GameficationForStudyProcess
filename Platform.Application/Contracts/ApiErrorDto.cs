namespace Platform.Application.Contracts;

public sealed record ApiErrorDto(
    string Code,
    string Message);

public static class ApiErrors
{
    public static readonly ApiErrorDto InvalidUserId =
        new("invalid_user_id", "Укажите корректный ID пользователя в формате GUID.");

    public static readonly ApiErrorDto InvalidCredentials =
        new("invalid_credentials", "Пользователь с таким ID не найден или его вход отключён.");

    public static readonly ApiErrorDto AuthenticationRequired =
        new("authentication_required", "Сессия отсутствует или завершилась. Выполните вход снова.");

    public static readonly ApiErrorDto CourseNotFound =
        new("course_not_found", "Курс за указанный учебный год не найден.");

    public static readonly ApiErrorDto CourseAccessDenied =
        new("course_access_denied", "У вас нет доступа к этому курсу.");

    public static readonly ApiErrorDto AccessDenied =
        new("access_denied", "У вашей роли нет доступа к этой операции.");

    public static readonly ApiErrorDto AchievementGraphTemplateNotFound =
        new("achievement_graph_template_not_found", "Шаблон графа достижений не найден. Обратитесь к администратору.");

    public static readonly ApiErrorDto AchievementGraphTemplateInvalid =
        new("achievement_graph_template_invalid", "Шаблон графа достижений содержит ошибку. Обратитесь к администратору.");

    public static readonly ApiErrorDto InternalServerError =
        new("internal_server_error", "Сервис временно недоступен. Повторите запрос позже.");
}
