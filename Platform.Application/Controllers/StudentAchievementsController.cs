using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform.Application.Authentication;
using Platform.Application.Contracts;
using Platform.Application.Services;

namespace Platform.Application.Controllers;

[ApiController]
[Authorize(Roles = "student")]
[Route("api/student/courses/{courseId:guid}/{year:int}/achievements")]
public sealed class StudentAchievementsController(
    IStudentAchievementGraphService studentAchievementGraphService) : ControllerBase
{
    /// <summary>
    /// Возвращает XML-граф достижений студента по выбранному курсу.
    /// </summary>
    /// <remarks>
    /// Сервис берёт XML-шаблон графа и выставляет статусы нод по данным из БД.
    /// Ноды сопоставляются с достижениями через атрибут `AchievementId` или временный
    /// вариант `AchivementId` в XML-шаблоне. Ответ предназначен для компонента,
    /// который будет отрисовывать граф на фронте.
    /// </remarks>
    /// <param name="courseId">ID курса.</param>
    /// <param name="year">Год экземпляра курса.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    /// <returns>XML-документ графа со статусами `earned`, `available` и `locked`.</returns>
    [HttpGet("graph")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK, "application/xml")]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status401Unauthorized, "application/json")]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status403Forbidden, "application/json")]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status404NotFound, "application/json")]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status500InternalServerError, "application/json")]
    public async Task<IActionResult> GetAchievementGraph(
        Guid courseId,
        int year,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var studentId))
            return Unauthorized(ApiErrors.AuthenticationRequired);

        var result = await studentAchievementGraphService.GetGraphXmlAsync(
            studentId,
            courseId,
            year,
            cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Повторно запускает цикл обработки достижений и возвращает обновлённый XML-граф.
    /// </summary>
    /// <remarks>
    /// Endpoint нужен кнопке "Обновить граф": сервер сначала проверяет доступ студента
    /// к курсу, затем заново прогоняет цикл выдачи достижений и возвращает тот же XML-граф,
    /// но уже со свежими статусами нод и рёбер.
    /// </remarks>
    /// <param name="courseId">ID курса.</param>
    /// <param name="year">Год экземпляра курса.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    /// <returns>Обновлённый XML-документ графа со статусами `earned`, `available` и `locked`.</returns>
    [HttpPost("graph/refresh")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK, "application/xml")]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status401Unauthorized, "application/json")]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status403Forbidden, "application/json")]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status404NotFound, "application/json")]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status500InternalServerError, "application/json")]
    public async Task<IActionResult> RefreshAchievementGraph(
        Guid courseId,
        int year,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var studentId))
            return Unauthorized(ApiErrors.AuthenticationRequired);

        var result = await studentAchievementGraphService.RefreshGraphXmlAsync(
            studentId,
            courseId,
            year,
            cancellationToken);

        return ToActionResult(result);
    }

    private IActionResult ToActionResult(StudentAchievementGraphQueryResult result)
    {
        return result.Status switch
        {
            StudentAchievementGraphQueryStatus.Success =>
                Content(result.Xml!, "application/xml", Encoding.UTF8),
            StudentAchievementGraphQueryStatus.StudentNotFound =>
                Unauthorized(ApiErrors.AuthenticationRequired),
            StudentAchievementGraphQueryStatus.CourseNotFound =>
                NotFound(ApiErrors.CourseNotFound),
            StudentAchievementGraphQueryStatus.AccessDenied =>
                StatusCode(StatusCodes.Status403Forbidden, ApiErrors.CourseAccessDenied),
            StudentAchievementGraphQueryStatus.TemplateNotFound =>
                StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiErrors.AchievementGraphTemplateNotFound),
            StudentAchievementGraphQueryStatus.InvalidTemplate =>
                StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiErrors.AchievementGraphTemplateInvalid),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }
}
