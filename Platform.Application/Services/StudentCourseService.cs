using Microsoft.EntityFrameworkCore;
using Platform.Application.Contracts;
using Platform.DataAccess.Postgress;

namespace Platform.Application.Services;

public sealed class StudentCourseService(
    PlatformDbContext dbContext,
    TimeProvider timeProvider) : IStudentCourseService
{
    public async Task<IReadOnlyList<CourseDto>> GetCoursesAsync(
        Guid studentId,
        CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;

        return await dbContext.CourseInstanceStudents
            .AsNoTracking()
            .Where(enrollment =>
                enrollment.PersonID == studentId &&
                enrollment.StartDate <= now &&
                (!enrollment.EndDate.HasValue || enrollment.EndDate.Value >= now))
            .OrderByDescending(enrollment => enrollment.Year)
            .ThenBy(enrollment => enrollment.CourseInstance.Course.Title)
            .Select(enrollment => new CourseDto(
                enrollment.CourseID,
                enrollment.CourseInstance.Course.Title,
                enrollment.CourseInstance.Course.Description,
                enrollment.Year))
            .ToListAsync(cancellationToken);
    }
}
