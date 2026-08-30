using Platform.Core.Models;

namespace Platform.Application.Services;

public interface IStaffCourseExportService
{
    Task<StaffCourseExportResult> CreateTeacherReportAsync(
        Guid userId,
        UserRole role,
        Guid courseId,
        int year,
        CancellationToken cancellationToken = default);

    Task<StaffCourseExportResult> CreateCourseArchiveAsync(
        Guid userId,
        UserRole role,
        Guid courseId,
        int year,
        CancellationToken cancellationToken = default);
}

public enum StaffCourseExportStatus
{
    Success,
    CourseNotFound,
    AccessDenied
}

public sealed record StaffCourseExportResult(
    StaffCourseExportStatus Status,
    byte[]? Content = null,
    string? ContentType = null,
    string? FileName = null);
