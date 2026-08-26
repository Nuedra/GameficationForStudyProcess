using Microsoft.EntityFrameworkCore;
using Platform.Lms;

namespace Platform.DataAccess.Postgress.Lms;

/// <summary>
/// Временная реализация контракта LMS поверх существующей локальной схемы.
/// После появления LMS заменяется адаптером без изменения прикладных сервисов.
/// </summary>
public sealed class LocalLmsDataSource(LocalLmsDbContext dbContext) : ILmsDataSource
{
    public Task<LmsPerson?> GetPersonAsync(
        Guid personId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Students
            .AsNoTracking()
            .Where(person => person.Id == personId)
            .Select(person => new LmsPerson(
                person.Id,
                person.Name,
                null,
                person.Surname,
                person.Group))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public Task<bool> CourseInstanceExistsAsync(
        Guid courseId,
        int year,
        CancellationToken cancellationToken = default)
    {
        return dbContext.CourseInstances
            .AsNoTracking()
            .AnyAsync(
                course => course.CourseID == courseId && course.Year == year,
                cancellationToken);
    }

    public Task<bool> HasActiveEnrollmentAsync(
        Guid personId,
        Guid courseId,
        int year,
        DateTimeOffset effectiveAt,
        CancellationToken cancellationToken = default)
    {
        var effectiveAtUtc = effectiveAt.UtcDateTime;

        return dbContext.CourseInstanceStudents
            .AsNoTracking()
            .AnyAsync(
                enrollment =>
                    enrollment.PersonID == personId &&
                    enrollment.CourseID == courseId &&
                    enrollment.Year == year &&
                    enrollment.StartDate <= effectiveAtUtc &&
                    (!enrollment.EndDate.HasValue || enrollment.EndDate.Value >= effectiveAtUtc),
                cancellationToken);
    }

    public async Task<IReadOnlyList<LmsCourseInstance>> GetActiveCourseInstancesAsync(
        Guid personId,
        DateTimeOffset effectiveAt,
        CancellationToken cancellationToken = default)
    {
        var effectiveAtUtc = effectiveAt.UtcDateTime;

        return await dbContext.CourseInstanceStudents
            .AsNoTracking()
            .Where(enrollment =>
                enrollment.PersonID == personId &&
                enrollment.StartDate <= effectiveAtUtc &&
                (!enrollment.EndDate.HasValue || enrollment.EndDate.Value >= effectiveAtUtc))
            .OrderByDescending(enrollment => enrollment.Year)
            .ThenBy(enrollment => enrollment.CourseInstance.Course.Title)
            .Select(enrollment => new LmsCourseInstance(
                enrollment.CourseID,
                enrollment.Year,
                enrollment.CourseInstance.Course.Title,
                enrollment.CourseInstance.ContentScopeID,
                enrollment.CourseInstance.Course.Description))
            .ToListAsync(cancellationToken);
    }
}
