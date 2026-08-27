using Platform.Application.Contracts;

namespace Platform.Application.Services;

public interface IUserSessionService
{
    Task<AuthenticatedUserDto?> SignInAsync(
        HttpContext httpContext,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task SignOutAsync(HttpContext httpContext);
}
