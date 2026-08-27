namespace Platform.Lms;

/// <summary>
/// Командный контракт управления назначениями преподавателей в LMS.
/// Локальная БД является временной реализацией; внешний LMS-адаптер должен
/// реализовать тот же контракт без изменения прикладного сервиса кабинета.
/// </summary>
public interface ILmsTeachingAssignmentDataSource
{
    Task<IReadOnlyList<LmsTeachingAssignment>> GetTeachingAssignmentsAsync(
        Guid courseId,
        int year,
        CancellationToken cancellationToken = default);

    Task<LmsTeachingAssignment> SaveTeachingAssignmentAsync(
        Guid courseId,
        int year,
        Guid teacherId,
        DateTimeOffset startDate,
        DateTimeOffset? endDate,
        bool isLead,
        CancellationToken cancellationToken = default);

    Task<bool> EndTeachingAssignmentAsync(
        Guid courseId,
        int year,
        Guid teacherId,
        DateTimeOffset endedAt,
        CancellationToken cancellationToken = default);
}

public sealed record LmsTeachingAssignment(
    Guid CourseId,
    int Year,
    Guid TeacherId,
    DateTimeOffset StartDate,
    DateTimeOffset? EndDate,
    bool IsLead);
