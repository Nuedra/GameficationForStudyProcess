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

    public static readonly ApiErrorDto TeacherNotFound =
        new("teacher_not_found", "Активный преподаватель с таким ID не найден.");

    public static readonly ApiErrorDto TeachingAssignmentNotFound =
        new("teaching_assignment_not_found", "Назначение преподавателя не найдено.");

    public static readonly ApiErrorDto TeachingAssignmentNotActive =
        new("teaching_assignment_not_active", "Можно завершить только действующее назначение.");

    public static readonly ApiErrorDto InvalidTeachingAssignmentPeriod =
        new("invalid_teaching_assignment_period", "Укажите дату начала; дата окончания должна быть позже неё.");

    public static readonly ApiErrorDto LeadTeachingAssignmentConflict =
        new("lead_teaching_assignment_conflict", "На пересекающийся период уже назначен ведущий преподаватель.");

    public static readonly ApiErrorDto AchievementNotFound =
        new("achievement_not_found", "Достижение в указанном экземпляре курса не найдено.");

    public static readonly ApiErrorDto AchievementCriteriaNotFound =
        new("achievement_criteria_not_found", "Критерий достижения не найден.");

    public static readonly ApiErrorDto AchievementAwardNotFound =
        new("achievement_award_not_found", "У указанного студента нет этой ачивки.");

    public static readonly ApiErrorDto StudentNotFound =
        new("student_not_found", "Студент с таким ID не найден.");

    public static readonly ApiErrorDto InvalidStudentId =
        new("invalid_student_id", "Укажите корректный ID студента в формате GUID.");

    public static readonly ApiErrorDto StudentCourseEnrollmentRequired =
        new("student_course_enrollment_required", "Студент не обучается на этом курсе в указанном году.");

    public static readonly ApiErrorDto AchievementAwardAlreadyExists =
        new("achievement_award_already_exists", "У указанного студента уже есть эта ачивка.");

    public static readonly ApiErrorDto AchievementPrerequisiteMissing =
        new("achievement_prerequisite_missing", "Студент должен получить одно из предыдущих достижений графа перед выдачей этой ачивки.");

    public static readonly ApiErrorDto InvalidAchievement =
        new("invalid_achievement", "Проверьте название, описание и трек достижения.");

    public static readonly ApiErrorDto InvalidAchievementCriteria =
        new("invalid_achievement_criteria", "Критерий должен содержать хотя бы один тег; теги разделяются запятыми.");

    public static readonly ApiErrorDto DuplicateAchievementTitle =
        new("duplicate_achievement_title", "В этом экземпляре курса уже есть достижение с таким названием.");

    public static readonly ApiErrorDto AchievementAwardsConfirmationRequired =
        new("achievement_awards_confirmation_required", "Подтвердите удаление достижения вместе с отзывом всех его выдач студентам.");

    public static readonly ApiErrorDto AchievementHasDependencies =
        new("achievement_has_dependencies", "Нельзя удалить достижение, пока оно участвует в зависимостях графа.");

    public static readonly ApiErrorDto AchievementGraphTemplateNotFound =
        new("achievement_graph_template_not_found", "Шаблон графа достижений не найден. Обратитесь к администратору.");

    public static readonly ApiErrorDto AchievementGraphTemplateInvalid =
        new("achievement_graph_template_invalid", "Шаблон графа достижений содержит ошибку. Обратитесь к администратору.");

    public static readonly ApiErrorDto InternalServerError =
        new("internal_server_error", "Сервис временно недоступен. Повторите запрос позже.");
}
