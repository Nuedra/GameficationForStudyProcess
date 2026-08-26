using Platform.Core.Models;

namespace Platform.Application.Authentication;

public sealed class GuidAuthenticationOptions
{
    public const string SectionName = "GuidAuthentication";

    public List<PrivilegedGuidUserOptions> PrivilegedUsers { get; set; } = [];
}

public sealed class PrivilegedGuidUserOptions
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
}
