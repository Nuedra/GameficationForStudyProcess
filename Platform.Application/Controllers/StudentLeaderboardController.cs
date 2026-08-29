using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform.Application.Authentication;
using Platform.Application.Contracts;
using Platform.Application.Services;

namespace Platform.Application.Controllers;

[ApiController]
[Authorize(Roles = "student")]
[Route("api/student/courses/{courseId:guid}/{year:int}/leaderboard")]
[Produces("application/json")]
public sealed class StudentLeaderboardController(
    IStudentLeaderboardService leaderboardService) : ControllerBase
{
    /// <summary>
    /// Возвращает таблицу лидеров по числу полученных достижений в выбранном курсе.
    /// </summary>
    /// <remarks>
    /// Студент видит только поток выбранного экземпляра курса, если сам активно
    /// обучается на этом курсе в указанном году.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<LeaderboardEntryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<LeaderboardEntryDto>>> GetLeaderboard(
        Guid courseId,
        int year,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var studentId))
            return Unauthorized(ApiErrors.AuthenticationRequired);

        var result = await leaderboardService.GetLeaderboardAsync(
            studentId,
            courseId,
            year,
            cancellationToken);

        return result.Status switch
        {
            StudentLeaderboardQueryStatus.Success => Ok(result.Entries),
            StudentLeaderboardQueryStatus.StudentNotFound =>
                Unauthorized(ApiErrors.AuthenticationRequired),
            StudentLeaderboardQueryStatus.CourseNotFound =>
                NotFound(ApiErrors.CourseNotFound),
            _ => StatusCode(
                StatusCodes.Status403Forbidden,
                ApiErrors.CourseAccessDenied)
        };
    }
}
