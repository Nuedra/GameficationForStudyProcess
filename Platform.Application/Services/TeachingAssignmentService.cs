using Microsoft.Extensions.Options;
using Platform.Application.Authentication;
using Platform.Application.Contracts;
using Platform.Core.Abstractions;
using Platform.Core.Models;
using Platform.Lms;

namespace Platform.Application.Services;

public sealed class TeachingAssignmentService(
    ILmsCourseManagementDataSource courseDataSource,
    ILmsTeachingAssignmentDataSource assignmentDataSource,
    IAccessPolicyService accessPolicy,
    IOptionsMonitor<GuidAuthenticationOptions> authenticationOptions,
    TimeProvider timeProvider,
    ILogger<TeachingAssignmentService> logger) : ITeachingAssignmentService
{
    public async Task<TeachingAssignmentResult> GetAsync(
        Guid userId,
        UserRole role,
        Guid courseId,
        int year,
        CancellationToken cancellationToken = default)
    {
        var status = await ValidateAdministratorAndCourseAsync(
            role,
            courseId,
            year,
            cancellationToken);
        if (status != TeachingAssignmentStatus.Success)
            return new TeachingAssignmentResult(status);

        return new TeachingAssignmentResult(
            TeachingAssignmentStatus.Success,
            await BuildManagementDtoAsync(courseId, year, cancellationToken));
    }

    public async Task<TeachingAssignmentResult> SaveAsync(
        Guid userId,
        UserRole role,
        Guid courseId,
        int year,
        Guid teacherId,
        SaveTeachingAssignmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var status = await ValidateAdministratorAndCourseAsync(
            role,
            courseId,
            year,
            cancellationToken);
        if (status != TeachingAssignmentStatus.Success)
            return new TeachingAssignmentResult(status);

        if (!GetActiveTeachers().Any(teacher => teacher.Id == teacherId))
            return new TeachingAssignmentResult(TeachingAssignmentStatus.TeacherNotFound);

        if (request.StartDate == default ||
            request.EndDate.HasValue && request.StartDate >= request.EndDate.Value)
            return new TeachingAssignmentResult(TeachingAssignmentStatus.InvalidPeriod);

        var assignments = await assignmentDataSource.GetTeachingAssignmentsAsync(
            courseId,
            year,
            cancellationToken);
        if (request.IsLead && assignments.Any(assignment =>
                assignment.TeacherId != teacherId &&
                assignment.IsLead &&
                PeriodsOverlap(
                    request.StartDate,
                    request.EndDate,
                    assignment.StartDate,
                    assignment.EndDate)))
        {
            return new TeachingAssignmentResult(
                TeachingAssignmentStatus.LeadAssignmentConflict);
        }

        await assignmentDataSource.SaveTeachingAssignmentAsync(
            courseId,
            year,
            teacherId,
            request.StartDate,
            request.EndDate,
            request.IsLead,
            cancellationToken);

        logger.LogInformation(
            "Administrator {AdministratorId} saved teaching assignment. CourseId={CourseId}, Year={Year}, TeacherId={TeacherId}, StartDate={StartDate}, EndDate={EndDate}, IsLead={IsLead}",
            userId,
            courseId,
            year,
            teacherId,
            request.StartDate,
            request.EndDate,
            request.IsLead);

        return new TeachingAssignmentResult(
            TeachingAssignmentStatus.Success,
            await BuildManagementDtoAsync(courseId, year, cancellationToken));
    }

    public async Task<TeachingAssignmentResult> EndAsync(
        Guid userId,
        UserRole role,
        Guid courseId,
        int year,
        Guid teacherId,
        CancellationToken cancellationToken = default)
    {
        var status = await ValidateAdministratorAndCourseAsync(
            role,
            courseId,
            year,
            cancellationToken);
        if (status != TeachingAssignmentStatus.Success)
            return new TeachingAssignmentResult(status);

        var now = timeProvider.GetUtcNow();
        var assignment = (await assignmentDataSource.GetTeachingAssignmentsAsync(
                courseId,
                year,
                cancellationToken))
            .SingleOrDefault(item => item.TeacherId == teacherId);

        if (assignment is null)
            return new TeachingAssignmentResult(TeachingAssignmentStatus.AssignmentNotFound);
        if (assignment.StartDate > now ||
            assignment.EndDate.HasValue && assignment.EndDate.Value < now)
        {
            return new TeachingAssignmentResult(TeachingAssignmentStatus.AssignmentNotActive);
        }

        await assignmentDataSource.EndTeachingAssignmentAsync(
            courseId,
            year,
            teacherId,
            now,
            cancellationToken);

        logger.LogInformation(
            "Administrator {AdministratorId} ended teaching assignment. CourseId={CourseId}, Year={Year}, TeacherId={TeacherId}, EndedAt={EndedAt}",
            userId,
            courseId,
            year,
            teacherId,
            now);

        return new TeachingAssignmentResult(
            TeachingAssignmentStatus.Success,
            await BuildManagementDtoAsync(courseId, year, cancellationToken));
    }

    private async Task<TeachingAssignmentStatus> ValidateAdministratorAndCourseAsync(
        UserRole role,
        Guid courseId,
        int year,
        CancellationToken cancellationToken)
    {
        if (role != UserRole.Administrator ||
            !accessPolicy.Can(role, Permission.ManageUsers) ||
            !accessPolicy.Can(role, Permission.ManageCourses))
        {
            return TeachingAssignmentStatus.AccessDenied;
        }

        return await courseDataSource.GetCourseInstanceAsync(
            courseId,
            year,
            cancellationToken) is null
                ? TeachingAssignmentStatus.CourseNotFound
                : TeachingAssignmentStatus.Success;
    }

    private async Task<TeachingAssignmentManagementDto> BuildManagementDtoAsync(
        Guid courseId,
        int year,
        CancellationToken cancellationToken)
    {
        var teachers = GetActiveTeachers();
        var namesById = teachers.ToDictionary(teacher => teacher.Id, teacher => teacher.DisplayName);
        var now = timeProvider.GetUtcNow();
        var assignments = await assignmentDataSource.GetTeachingAssignmentsAsync(
            courseId,
            year,
            cancellationToken);

        return new TeachingAssignmentManagementDto(
            courseId,
            year,
            teachers,
            assignments.Select(assignment => new TeachingAssignmentDto(
                    assignment.TeacherId,
                    namesById.GetValueOrDefault(
                        assignment.TeacherId,
                        $"Преподаватель {assignment.TeacherId}"),
                    assignment.StartDate,
                    assignment.EndDate,
                    assignment.IsLead,
                    assignment.StartDate <= now &&
                    (!assignment.EndDate.HasValue || assignment.EndDate.Value >= now)))
                .ToList());
    }

    private IReadOnlyList<TeacherOptionDto> GetActiveTeachers()
    {
        return authenticationOptions.CurrentValue.PrivilegedUsers
            .Where(user => user.IsActive && user.Role == UserRole.Teacher)
            .OrderBy(user => user.DisplayName)
            .Select(user => new TeacherOptionDto(user.Id, user.DisplayName))
            .ToList();
    }

    private static bool PeriodsOverlap(
        DateTimeOffset firstStart,
        DateTimeOffset? firstEnd,
        DateTimeOffset secondStart,
        DateTimeOffset? secondEnd)
    {
        return (!firstEnd.HasValue || secondStart <= firstEnd.Value) &&
               (!secondEnd.HasValue || firstStart <= secondEnd.Value);
    }
}
