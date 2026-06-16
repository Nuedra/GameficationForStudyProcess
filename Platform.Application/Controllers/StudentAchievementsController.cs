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
    IStudentAchievementService studentAchievementService,
    IStudentAchievementGraphService studentAchievementGraphService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<StudentAchievementsDto>> GetAchievements(
        Guid courseId,
        int year,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var studentId))
            return Unauthorized(ApiErrors.AuthenticationRequired);

        var result = await studentAchievementService.GetEarnedAchievementsAsync(
            studentId,
            courseId,
            year,
            cancellationToken);

        return result.Status switch
        {
            StudentAchievementsQueryStatus.Success => Ok(result.Data),
            StudentAchievementsQueryStatus.StudentNotFound =>
                Unauthorized(ApiErrors.AuthenticationRequired),
            StudentAchievementsQueryStatus.CourseNotFound =>
                NotFound(ApiErrors.CourseNotFound),
            StudentAchievementsQueryStatus.AccessDenied =>
                StatusCode(StatusCodes.Status403Forbidden, ApiErrors.CourseAccessDenied),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [HttpGet("graph")]
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
