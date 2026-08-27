namespace Platform.Lms;

/// <summary>
/// Предоставляет необходимые кабинету преподавателя и администратора
/// read-only сведения о запусках курсов и преподавательских назначениях.
/// </summary>
public interface ILmsCourseManagementDataSource
{
    Task<IReadOnlyList<LmsCourseInstance>> GetAllCourseInstancesAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LmsCourseInstance>> GetAssignedCourseInstancesAsync(
        Guid teacherId,
        DateTimeOffset effectiveAt,
        CancellationToken cancellationToken = default);

    Task<LmsCourseInstance?> GetCourseInstanceAsync(
        Guid courseId,
        int year,
        CancellationToken cancellationToken = default);

    Task<bool> HasActiveTeachingAssignmentAsync(
        Guid teacherId,
        Guid courseId,
        int year,
        DateTimeOffset effectiveAt,
        CancellationToken cancellationToken = default);
}
