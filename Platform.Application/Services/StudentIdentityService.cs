using Microsoft.EntityFrameworkCore;
using Platform.Application.Contracts;
using Platform.DataAccess.Postgress;

namespace Platform.Application.Services;

public sealed class StudentIdentityService(PlatformDbContext dbContext) : IStudentIdentityService
{
    public Task<StudentDto?> FindByIdAsync(
        Guid studentId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Students
            .AsNoTracking()
            .Where(student => student.Id == studentId)
            .Select(student => new StudentDto(
                student.Id,
                student.Surname + " " + student.Name,
                student.Group))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
