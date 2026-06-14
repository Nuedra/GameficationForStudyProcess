using Platform.Application.Contracts;

namespace Platform.Application.Services;

public interface IStudentIdentityService
{
    Task<StudentDto?> FindByIdAsync(
        Guid studentId,
        CancellationToken cancellationToken = default);
}
