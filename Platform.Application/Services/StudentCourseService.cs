using Platform.Application.Contracts;
using Platform.Lms;

namespace Platform.Application.Services;

public sealed class StudentCourseService(
    ILmsDataSource lmsDataSource,
    TimeProvider timeProvider) : IStudentCourseService
{
    public async Task<IReadOnlyList<CourseDto>> GetCoursesAsync(
        Guid studentId,
        CancellationToken cancellationToken = default)
    {
        var courses = await lmsDataSource.GetActiveCourseInstancesAsync(
            studentId,
            timeProvider.GetUtcNow(),
            cancellationToken);

        return courses
            .Select(course => new CourseDto(
                course.CourseId,
                course.Name,
                course.Description ?? string.Empty,
                course.Year))
            .ToList();
    }
}
