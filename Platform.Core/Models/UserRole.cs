namespace Platform.Core.Models;

public enum UserRole
{
    Student = 0,
    Teacher = 1,
    Administrator = 2
}

public static class UserRoleDictionary
{
    public static readonly IReadOnlyDictionary<UserRole, string> Values =
        new Dictionary<UserRole, string>
        {
            [UserRole.Student] = "student",
            [UserRole.Teacher] = "teacher",
            [UserRole.Administrator] = "administrator"
        };
}
