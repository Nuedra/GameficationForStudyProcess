using Microsoft.Extensions.Options;
using Platform.Application.Authentication;
using Platform.Application.Models;
using Platform.Core.Models;
using Platform.Lms;

namespace Platform.Application.Services;

public sealed class UserIdentityService(
    ILmsDataSource lmsDataSource,
    IOptionsMonitor<GuidAuthenticationOptions> options,
    ILogger<UserIdentityService> logger) : IUserIdentityService
{
    public async Task<ResolvedUserIdentity?> ResolveByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var privilegedUser = options.CurrentValue.PrivilegedUsers
            .SingleOrDefault(user => user.Id == userId && user.IsActive);
        var person = await lmsDataSource.GetPersonAsync(userId, cancellationToken);

        if (privilegedUser is not null && person is not null)
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

        return person is null
            ? null
            : new ResolvedUserIdentity(
                person.Id,
                person.DisplayName,
                UserRole.Student,
                person.CurrentEducationalGroupName);
    }
}
