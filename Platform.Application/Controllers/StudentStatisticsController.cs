using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform.Application.Authentication;
using Platform.Application.Contracts;
using Platform.Application.Services;

namespace Platform.Application.Controllers;

[ApiController]
[Authorize(Roles = "student")]
[Route("api/student")]
[Produces("application/json")]
public sealed class StudentStatisticsController(
    IStudentStatisticsService statisticsService) : ControllerBase
{
    /// <summary>
    /// Возвращает статистику студента по всем его текущим курсам.
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(StudentStatisticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<StudentStatisticsDto>> GetCurrentCoursesStatistics(
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var studentId))
            return Unauthorized(ApiErrors.AuthenticationRequired);

        var result = await statisticsService.GetCurrentCoursesStatisticsAsync(
            studentId,
            cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Возвращает статистику студента по выбранному текущему курсу.
    /// </summary>
    [HttpGet("courses/{courseId:guid}/{year:int}/statistics")]
    [ProducesResponseType(typeof(StudentStatisticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudentStatisticsDto>> GetCourseStatistics(
        Guid courseId,
        int year,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var studentId))
            return Unauthorized(ApiErrors.AuthenticationRequired);

        var result = await statisticsService.GetCourseStatisticsAsync(
            studentId,
            courseId,
            year,
            cancellationToken);
        return ToActionResult(result);
    }

    private ActionResult<StudentStatisticsDto> ToActionResult(
        StudentStatisticsQueryResult result)
    {
        return result.Status switch
        {
            StudentStatisticsQueryStatus.Success => Ok(result.Statistics),
            StudentStatisticsQueryStatus.StudentNotFound =>
                Unauthorized(ApiErrors.AuthenticationRequired),
            StudentStatisticsQueryStatus.CourseNotFound =>
                NotFound(ApiErrors.CourseNotFound),
            _ => StatusCode(
                StatusCodes.Status403Forbidden,
                ApiErrors.CourseAccessDenied)
        };
    }
}
