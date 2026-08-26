using Platform.Core.Models;

namespace Platform.Application.Models;

public sealed record ResolvedUserIdentity(
    Guid Id,
    string DisplayName,
    UserRole Role,
    string? Group);
