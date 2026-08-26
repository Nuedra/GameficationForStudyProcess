using Platform.Application.Contracts;
using Platform.Core.Models;

namespace Platform.Application.Services;

public interface ITeachingAssignmentService
{
    Task<TeachingAssignmentResult> GetAsync(
        Guid userId,
        UserRole role,
        Guid courseId,
        int year,
        CancellationToken cancellationToken = default);

    Task<TeachingAssignmentResult> SaveAsync(
        Guid userId,
        UserRole role,
        Guid courseId,
        int year,
        Guid teacherId,
        SaveTeachingAssignmentRequest request,
        CancellationToken cancellationToken = default);

    Task<TeachingAssignmentResult> EndAsync(
        Guid userId,
        UserRole role,
        Guid courseId,
        int year,
        Guid teacherId,
        CancellationToken cancellationToken = default);
}

public enum TeachingAssignmentStatus
{
    Success,
    AccessDenied,
    CourseNotFound,
    TeacherNotFound,
    AssignmentNotFound,
    AssignmentNotActive,
    InvalidPeriod,
    LeadAssignmentConflict
}

public sealed record TeachingAssignmentResult(
    TeachingAssignmentStatus Status,
    TeachingAssignmentManagementDto? Management = null);
