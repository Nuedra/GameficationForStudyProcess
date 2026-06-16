using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform.Application.Authentication;
using Platform.Application.Contracts;
using Platform.Application.Services;

namespace Platform.Application.Controllers;

[ApiController]
[Authorize(Roles = "student")]
[Route("api/student/courses")]
[Produces("application/json")]
public sealed class StudentCoursesController(IStudentCourseService studentCourseService)
    : ControllerBase
{
    /// <summary>
    /// Возвращает курсы текущего студента.
    /// </summary>
    /// <remarks>
    /// В ответ попадают только активные экземпляры курсов, на которые записан
    /// авторизованный студент. Фронт использует этот список для выбора курса перед
    /// запросом достижений или XML-графа.
    /// </remarks>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    /// <returns>Список доступных студенту курсов.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CourseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<CourseDto>>> GetCourses(
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var studentId))
            return Unauthorized(ApiErrors.AuthenticationRequired);

        var courses = await studentCourseService.GetCoursesAsync(
            studentId,
            cancellationToken);

        return Ok(courses);
    }
}
