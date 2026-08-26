using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Platform.Application.Authentication;
using Platform.Application.Models;
using Platform.Core.Models;
using Platform.DataAccess.Postgress;

namespace Platform.Application.Services;

public sealed class UserIdentityService(
    PlatformDbContext dbContext,
    IOptionsMonitor<GuidAuthenticationOptions> options,
    ILogger<UserIdentityService> logger) : IUserIdentityService
{
    public async Task<ResolvedUserIdentity?> ResolveByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var privilegedUser = options.CurrentValue.PrivilegedUsers
            .SingleOrDefault(user => user.Id == userId && user.IsActive);
        var student = await dbContext.Students
            .AsNoTracking()
            .Where(item => item.Id == userId)
            .Select(item => new
            {
                item.Id,
                DisplayName = item.Surname + " " + item.Name,
                item.Group
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (privilegedUser is not null && student is not null)
        {
            logger.LogError(
                "GUID {UserId} одновременно назначен студенту и привилегированному пользователю. Вход отклонён.",
                userId);
            return null;
        }

        if (privilegedUser is not null)
        {
            return new ResolvedUserIdentity(
                privilegedUser.Id,
                privilegedUser.DisplayName,
                privilegedUser.Role,
                Group: null);
        }

        return student is null
            ? null
            : new ResolvedUserIdentity(
                student.Id,
                student.DisplayName,
                UserRole.Student,
                student.Group);
    }
}
