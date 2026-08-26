using Platform.Core.Abstractions;
using Platform.Core.Models;

namespace Platform.Core.Policies;

public sealed class AccessPolicyService : IAccessPolicyService
{
    private static readonly IReadOnlyDictionary<UserRole, IReadOnlySet<Permission>> PermissionsByRole =
        new Dictionary<UserRole, IReadOnlySet<Permission>>
        {
            [UserRole.Student] = new HashSet<Permission>
            {
                Permission.ViewOwnAchievements
            },
            [UserRole.Teacher] = new HashSet<Permission>
            {
                Permission.ViewOwnAchievements,
                Permission.ManageCourses,
                Permission.EditAchievementCriteria
            },
            [UserRole.Administrator] = new HashSet<Permission>
            {
                Permission.ViewOwnAchievements,
                Permission.ManageCourses,
                Permission.EditAchievementCriteria,
                Permission.ManageUsers
            }
        };

    public bool Can(UserRole role, Permission permission)
    {
        return PermissionsByRole.TryGetValue(role, out var permissions) &&
               permissions.Contains(permission);
    }
}
