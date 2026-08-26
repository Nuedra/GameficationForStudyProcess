using System.Security.Claims;
using Platform.Core.Models;

namespace Platform.Application.Authentication;

public static class ClaimsPrincipalExtensions
{
    public static bool TryGetUserId(this ClaimsPrincipal principal, out Guid userId)
    {
        return Guid.TryParse(
            principal.FindFirstValue(ClaimTypes.NameIdentifier),
            out userId);
    }

    public static bool TryGetUserRole(this ClaimsPrincipal principal, out UserRole role)
    {
        var roleClaim = principal.FindFirstValue(ClaimTypes.Role);
        foreach (var pair in UserRoleDictionary.Values)
        {
            if (string.Equals(pair.Value, roleClaim, StringComparison.Ordinal))
            {
                role = pair.Key;
                return true;
            }
        }

        role = default;
        return false;
    }
}
