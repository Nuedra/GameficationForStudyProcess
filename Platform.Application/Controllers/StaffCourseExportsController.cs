using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform.Application.Authentication;
using Platform.Application.Contracts;
using Platform.Application.Services;

namespace Platform.Application.Controllers;

[ApiController]
[Authorize(Roles = "teacher,administrator")]
[Route("api/staff/courses/{courseId:guid}/{year:int}/exports")]
public sealed class StaffCourseExportsController(
    IStaffCourseExportService exportService) : ControllerBase
{
    /// <summary>
    /// Возвращает табличный CSV-отчёт по достижениям активных студентов курса.
    /// </summary>
    [HttpGet("teacher-report.csv")]
    [Authorize(Roles = "teacher")]
    [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK, "text/csv")]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadTeacherReport(
        Guid courseId,
        int year,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId) || !User.TryGetUserRole(out var role))
            return Unauthorized(ApiErrors.AuthenticationRequired);

        var result = await exportService.CreateTeacherReportAsync(
            userId,
            role,
            courseId,
            year,
            cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Возвращает полный ZIP-пакет курса с данными достижений и описанием формата.
    /// </summary>
    [HttpGet("archive.zip")]
    [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK, "application/zip")]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadArchive(
        Guid courseId,
        int year,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId) || !User.TryGetUserRole(out var role))
            return Unauthorized(ApiErrors.AuthenticationRequired);

        var result = await exportService.CreateCourseArchiveAsync(
            userId,
            role,
            courseId,
            year,
            cancellationToken);
        return ToActionResult(result);
    }

    private IActionResult ToActionResult(StaffCourseExportResult result)
    {
        return result.Status switch
        {
            StaffCourseExportStatus.Success => File(
                result.Content!,
                result.ContentType!,
                result.FileName!),
            StaffCourseExportStatus.CourseNotFound => NotFound(ApiErrors.CourseNotFound),
            _ => StatusCode(StatusCodes.Status403Forbidden, ApiErrors.CourseAccessDenied)
        };
    }
}
