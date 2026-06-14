using Platform.Application.Contracts;

namespace Platform.Application.Services;

public interface IStudentCourseService
{
    Task<IReadOnlyList<CourseDto>> GetCoursesAsync(
        Guid studentId,
        CancellationToken cancellationToken = default);
}
