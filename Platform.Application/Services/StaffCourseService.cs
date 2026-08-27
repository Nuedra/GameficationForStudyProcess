using Platform.Application.Contracts;
using Platform.Core.Abstractions;
using Platform.Core.Models;
using Platform.Lms;

namespace Platform.Application.Services;

public sealed class StaffCourseService(
    ILmsCourseManagementDataSource lmsDataSource,
    IAccessPolicyService accessPolicy,
    TimeProvider timeProvider) : IStaffCourseService
{
    public async Task<StaffCourseListResult> GetCoursesAsync(
        Guid userId,
        UserRole role,
        CancellationToken cancellationToken = default)
    {
        if (!accessPolicy.Can(role, Permission.ManageCourses))
        {
            return new StaffCourseListResult(
                StaffCourseQueryStatus.AccessDenied,
                []);
        }

        var courses = role == UserRole.Administrator
            ? await lmsDataSource.GetAllCourseInstancesAsync(cancellationToken)
            : await lmsDataSource.GetAssignedCourseInstancesAsync(
                userId,
                timeProvider.GetUtcNow(),
                cancellationToken);

        return new StaffCourseListResult(
            StaffCourseQueryStatus.Success,
            courses.Select(ToDto).ToList());
    }

    public async Task<StaffCourseQueryResult> GetCourseAsync(
        Guid userId,
        UserRole role,
        Guid courseId,
        int year,
        CancellationToken cancellationToken = default)
    {
        if (!accessPolicy.Can(role, Permission.ManageCourses))
            return new StaffCourseQueryResult(StaffCourseQueryStatus.AccessDenied);

        var course = await lmsDataSource.GetCourseInstanceAsync(
            courseId,
            year,
            cancellationToken);
        if (course is null)
            return new StaffCourseQueryResult(StaffCourseQueryStatus.CourseNotFound);

        if (role != UserRole.Administrator &&
            !await lmsDataSource.HasActiveTeachingAssignmentAsync(
                userId,
                courseId,
                year,
                timeProvider.GetUtcNow(),
                cancellationToken))
        {
            return new StaffCourseQueryResult(StaffCourseQueryStatus.AccessDenied);
        }

        return new StaffCourseQueryResult(
            StaffCourseQueryStatus.Success,
            ToDto(course));
    }

    private static CourseDto ToDto(LmsCourseInstance course)
    {
        return new CourseDto(
            course.CourseId,
            course.Name,
            course.Description ?? string.Empty,
            course.Year);
    }
}
