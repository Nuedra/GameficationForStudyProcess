using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform.Application.Authentication;
using Platform.Application.Contracts;
using Platform.Application.Services;

namespace Platform.Application.Controllers;

[ApiController]
[Authorize(Roles = "teacher,administrator")]
[Route("api/staff/courses/{courseId:guid}/{year:int}/achievements")]
[Produces("application/json")]
public sealed class AchievementManagementController(
    IAchievementManagementService achievementManagementService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ManagedAchievementDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ManagedAchievementDto>>> GetAll(
        Guid courseId,
        int year,
        CancellationToken cancellationToken)
    {
        if (!TryGetIdentity(out var userId, out var roleResult))
            return Unauthorized(ApiErrors.AuthenticationRequired);

        var result = await achievementManagementService.GetAllAsync(
            userId,
            roleResult,
            courseId,
            year,
            cancellationToken);
        return result.Status == AchievementManagementStatus.Success
            ? Ok(result.Achievements)
            : ToErrorResult(result.Status);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ManagedAchievementDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ManagedAchievementDto>> Create(
        Guid courseId,
        int year,
        [FromBody] SaveAchievementRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetIdentity(out var userId, out var roleResult))
            return Unauthorized(ApiErrors.AuthenticationRequired);

        var result = await achievementManagementService.CreateAsync(
            userId,
            roleResult,
            courseId,
            year,
            request,
            cancellationToken);
        return result.Status == AchievementManagementStatus.Success
            ? StatusCode(StatusCodes.Status201Created, result.Achievement)
            : ToErrorResult(result.Status);
    }

    [HttpPut("{achievementId:guid}")]
    [ProducesResponseType(typeof(ManagedAchievementDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ManagedAchievementDto>> Update(
        Guid courseId,
        int year,
        Guid achievementId,
        [FromBody] SaveAchievementRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetIdentity(out var userId, out var roleResult))
            return Unauthorized(ApiErrors.AuthenticationRequired);

        var result = await achievementManagementService.UpdateAsync(
            userId,
            roleResult,
            courseId,
            year,
            achievementId,
            request,
            cancellationToken);
        return result.Status == AchievementManagementStatus.Success
            ? Ok(result.Achievement)
            : ToErrorResult(result.Status);
    }

    [HttpDelete("{achievementId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(
        Guid courseId,
        int year,
        Guid achievementId,
        [FromQuery] bool revokeAwards,
        CancellationToken cancellationToken)
    {
        if (!TryGetIdentity(out var userId, out var roleResult))
            return Unauthorized(ApiErrors.AuthenticationRequired);

        var result = await achievementManagementService.DeleteAsync(
            userId,
            roleResult,
            courseId,
            year,
            achievementId,
            revokeAwards,
            cancellationToken);
        return result.Status == AchievementManagementStatus.Success
            ? NoContent()
            : ToErrorResult(result.Status);
    }

    [HttpPut("{achievementId:guid}/criteria")]
    [ProducesResponseType(typeof(ManagedAchievementDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ManagedAchievementDto>> SaveCriteria(
        Guid courseId,
        int year,
        Guid achievementId,
        [FromBody] SaveAchievementCriteriaRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetIdentity(out var userId, out var roleResult))
            return Unauthorized(ApiErrors.AuthenticationRequired);

        var result = await achievementManagementService.SaveCriteriaAsync(
            userId,
            roleResult,
            courseId,
            year,
            achievementId,
            request,
            cancellationToken);
        return result.Status == AchievementManagementStatus.Success
            ? Ok(result.Achievement)
            : ToErrorResult(result.Status);
    }

    [HttpDelete("{achievementId:guid}/criteria")]
    [ProducesResponseType(typeof(ManagedAchievementDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ManagedAchievementDto>> DeleteCriteria(
        Guid courseId,
        int year,
        Guid achievementId,
        CancellationToken cancellationToken)
    {
        if (!TryGetIdentity(out var userId, out var roleResult))
            return Unauthorized(ApiErrors.AuthenticationRequired);

        var result = await achievementManagementService.DeleteCriteriaAsync(
            userId,
            roleResult,
            courseId,
            year,
            achievementId,
            cancellationToken);
        return result.Status == AchievementManagementStatus.Success
            ? Ok(result.Achievement)
            : ToErrorResult(result.Status);
    }

    private bool TryGetIdentity(
        out Guid userId,
        out Platform.Core.Models.UserRole role)
    {
        role = default;
        return User.TryGetUserId(out userId) && User.TryGetUserRole(out role);
    }

    private ActionResult ToErrorResult(AchievementManagementStatus status)
    {
        return status switch
        {
            AchievementManagementStatus.AccessDenied => StatusCode(
                StatusCodes.Status403Forbidden,
                ApiErrors.CourseAccessDenied),
            AchievementManagementStatus.CourseNotFound => NotFound(ApiErrors.CourseNotFound),
            AchievementManagementStatus.AchievementNotFound =>
                NotFound(ApiErrors.AchievementNotFound),
            AchievementManagementStatus.CriteriaNotFound =>
                NotFound(ApiErrors.AchievementCriteriaNotFound),
            AchievementManagementStatus.InvalidAchievement =>
                BadRequest(ApiErrors.InvalidAchievement),
            AchievementManagementStatus.InvalidCriteria =>
                BadRequest(ApiErrors.InvalidAchievementCriteria),
            AchievementManagementStatus.DuplicateTitle => Conflict(
                ApiErrors.DuplicateAchievementTitle),
            AchievementManagementStatus.AwardsConfirmationRequired => Conflict(
                ApiErrors.AchievementAwardsConfirmationRequired),
            AchievementManagementStatus.HasDependencies => Conflict(
                ApiErrors.AchievementHasDependencies),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiErrors.InternalServerError)
        };
    }
}
