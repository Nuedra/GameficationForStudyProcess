using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform.Application.Authentication;
using Platform.Application.Contracts;
using Platform.Application.Services;

namespace Platform.Application.Controllers;

[ApiController]
[Authorize(Roles = "administrator")]
[Route("api/admin/courses/{courseId:guid}/{year:int}/teachers")]
[Produces("application/json")]
public sealed class TeachingAssignmentsController(
    ITeachingAssignmentService teachingAssignmentService) : ControllerBase
{
    /// <summary>
    /// Возвращает назначения и список доступных преподавателей курса.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(TeachingAssignmentManagementDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TeachingAssignmentManagementDto>> Get(
        Guid courseId,
        int year,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId) || !User.TryGetUserRole(out var role))
            return Unauthorized(ApiErrors.AuthenticationRequired);

        var result = await teachingAssignmentService.GetAsync(
            userId,
            role,
            courseId,
            year,
            cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Создаёт или изменяет назначение преподавателя на экземпляр курса.
    /// </summary>
    [HttpPut("{teacherId:guid}")]
    [ProducesResponseType(typeof(TeachingAssignmentManagementDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TeachingAssignmentManagementDto>> Save(
        Guid courseId,
        int year,
        Guid teacherId,
        [FromBody] SaveTeachingAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId) || !User.TryGetUserRole(out var role))
            return Unauthorized(ApiErrors.AuthenticationRequired);

        var result = await teachingAssignmentService.SaveAsync(
            userId,
            role,
            courseId,
            year,
            teacherId,
            request,
            cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Завершает действующее назначение текущим моментом, не удаляя запись.
    /// </summary>
    [HttpPost("{teacherId:guid}/end")]
    [ProducesResponseType(typeof(TeachingAssignmentManagementDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TeachingAssignmentManagementDto>> End(
        Guid courseId,
        int year,
        Guid teacherId,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId) || !User.TryGetUserRole(out var role))
            return Unauthorized(ApiErrors.AuthenticationRequired);

        var result = await teachingAssignmentService.EndAsync(
            userId,
            role,
            courseId,
            year,
            teacherId,
            cancellationToken);
        return ToActionResult(result);
    }

    private ActionResult<TeachingAssignmentManagementDto> ToActionResult(
        TeachingAssignmentResult result)
    {
        return result.Status switch
        {
            TeachingAssignmentStatus.Success => Ok(result.Management),
            TeachingAssignmentStatus.AccessDenied => StatusCode(
                StatusCodes.Status403Forbidden,
                ApiErrors.AccessDenied),
            TeachingAssignmentStatus.CourseNotFound => NotFound(ApiErrors.CourseNotFound),
            TeachingAssignmentStatus.TeacherNotFound => NotFound(ApiErrors.TeacherNotFound),
            TeachingAssignmentStatus.AssignmentNotFound =>
                NotFound(ApiErrors.TeachingAssignmentNotFound),
            TeachingAssignmentStatus.AssignmentNotActive =>
                BadRequest(ApiErrors.TeachingAssignmentNotActive),
            TeachingAssignmentStatus.InvalidPeriod =>
                BadRequest(ApiErrors.InvalidTeachingAssignmentPeriod),
            TeachingAssignmentStatus.LeadAssignmentConflict => Conflict(
                ApiErrors.LeadTeachingAssignmentConflict),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiErrors.InternalServerError)
        };
    }
}
