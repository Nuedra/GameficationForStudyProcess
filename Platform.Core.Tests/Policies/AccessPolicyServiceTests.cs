using Platform.Core.Models;
using Platform.Core.Policies;

namespace Platform.Core.Tests.Policies;

public sealed class AccessPolicyServiceTests
{
    private readonly AccessPolicyService _policy = new();

    [Fact]
    public void Student_CannotManageCoursesOrUsers()
    {
        Assert.True(_policy.Can(UserRole.Student, Permission.ViewOwnAchievements));
        Assert.False(_policy.Can(UserRole.Student, Permission.ManageCourses));
        Assert.False(_policy.Can(UserRole.Student, Permission.ManageUsers));
    }

    [Fact]
    public void Teacher_CanManageCoursesButCannotManageUsers()
    {
        Assert.True(_policy.Can(UserRole.Teacher, Permission.ManageCourses));
        Assert.True(_policy.Can(UserRole.Teacher, Permission.EditAchievementCriteria));
        Assert.True(_policy.Can(UserRole.Teacher, Permission.ViewAchievementAudit));
        Assert.False(_policy.Can(UserRole.Teacher, Permission.ManageUsers));
    }

    [Fact]
    public void Administrator_HasEveryDefinedPermission()
    {
        foreach (var permission in Enum.GetValues<Permission>())
            Assert.True(_policy.Can(UserRole.Administrator, permission));
    }
}
