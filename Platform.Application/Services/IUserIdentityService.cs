using Platform.Application.Models;

namespace Platform.Application.Services;

public interface IUserIdentityService
{
    Task<ResolvedUserIdentity?> ResolveByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
