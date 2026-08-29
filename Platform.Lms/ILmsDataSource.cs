namespace Platform.Lms;

/// <summary>
/// Предоставляет принадлежащие LMS сведения, необходимые подсистеме достижений.
/// Реализация может читать временную локальную модель или будущую базу LMS.
/// </summary>
public interface ILmsDataSource
{
    Task<LmsPerson?> GetPersonAsync(
        Guid personId,
        CancellationToken cancellationToken = default);

    Task<bool> CourseInstanceExistsAsync(
        Guid courseId,
        int year,
        CancellationToken cancellationToken = default);

    Task<bool> HasActiveEnrollmentAsync(
        Guid personId,
        Guid courseId,
        int year,
        DateTimeOffset effectiveAt,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LmsCourseInstance>> GetActiveCourseInstancesAsync(
        Guid personId,
        DateTimeOffset effectiveAt,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LmsCourseStudent>> GetActiveCourseInstanceStudentsAsync(
        Guid courseId,
        int year,
        DateTimeOffset effectiveAt,
        CancellationToken cancellationToken = default);
}
