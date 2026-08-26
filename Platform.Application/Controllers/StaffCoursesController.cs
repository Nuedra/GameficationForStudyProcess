using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform.Application.Authentication;
using Platform.Application.Contracts;
using Platform.Application.Services;

namespace Platform.Application.Controllers;

[ApiController]
[Authorize(Roles = "teacher,administrator")]
[Route("api/staff/courses")]
[Produces("application/json")]
public sealed class StaffCoursesController(IStaffCourseService staffCourseService)
    : ControllerBase
{
    /// <summary>
    /// Возвращает доступные сотруднику экземпляры курсов.
    /// </summary>
    /// <remarks>
    /// Преподаватель получает только активные назначения из LMS, администратор —
    /// все экземпляры. Endpoint не изменяет LMS-данные.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CourseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<CourseDto>>> GetCourses(
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId) || !User.TryGetUserRole(out var role))
            return Unauthorized(ApiErrors.AuthenticationRequired);

        var result = await staffCourseService.GetCoursesAsync(
            userId,
            role,
            cancellationToken);

        return result.Status == StaffCourseQueryStatus.Success
            ? Ok(result.Courses)
            : StatusCode(StatusCodes.Status403Forbidden, ApiErrors.AccessDenied);
    }

    /// <summary>
    /// Возвращает один экземпляр курса после объектной проверки доступа.
    /// </summary>
    [HttpGet("{courseId:guid}/{year:int}")]
    [ProducesResponseType(typeof(CourseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CourseDto>> GetCourse(
        Guid courseId,
        int year,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId) || !User.TryGetUserRole(out var role))
            return Unauthorized(ApiErrors.AuthenticationRequired);

        var result = await staffCourseService.GetCourseAsync(
            userId,
            role,
            courseId,
            year,
            cancellationToken);

        return result.Status switch
        {
            StaffCourseQueryStatus.Success => Ok(result.Course),
            StaffCourseQueryStatus.CourseNotFound =>
                NotFound(ApiErrors.CourseNotFound),
            _ => StatusCode(
                StatusCodes.Status403Forbidden,
                ApiErrors.CourseAccessDenied)
        };
    }
}
