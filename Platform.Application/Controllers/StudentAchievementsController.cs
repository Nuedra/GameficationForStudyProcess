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
    IStudentAchievementService studentAchievementService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<StudentAchievementsDto>> GetAchievements(
        Guid courseId,
        int year,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var studentId))
            return Unauthorized();

        var result = await studentAchievementService.GetEarnedAchievementsAsync(
            studentId,
            courseId,
            year,
            cancellationToken);

        return result.Status switch
        {
            StudentAchievementsQueryStatus.Success => Ok(result.Data),
            StudentAchievementsQueryStatus.StudentNotFound => Unauthorized(),
            StudentAchievementsQueryStatus.CourseNotFound => NotFound(),
            StudentAchievementsQueryStatus.AccessDenied => Forbid(),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }
}
