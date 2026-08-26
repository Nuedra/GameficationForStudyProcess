using Platform.Application.Contracts;
using Platform.Core.Models;

namespace Platform.Application.Services;

public interface IStaffCourseService
{
    Task<StaffCourseListResult> GetCoursesAsync(
        Guid userId,
        UserRole role,
        CancellationToken cancellationToken = default);

    Task<StaffCourseQueryResult> GetCourseAsync(
        Guid userId,
        UserRole role,
        Guid courseId,
        int year,
        CancellationToken cancellationToken = default);
}

public enum StaffCourseQueryStatus
{
    Success,
    CourseNotFound,
    AccessDenied
}

public sealed record StaffCourseListResult(
    StaffCourseQueryStatus Status,
    IReadOnlyList<CourseDto> Courses);

public sealed record StaffCourseQueryResult(
    StaffCourseQueryStatus Status,
    CourseDto? Course = null);
