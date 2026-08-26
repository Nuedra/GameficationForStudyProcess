using Platform.Core.Models;

namespace Platform.Application.Contracts;

public sealed record AuthenticatedUserDto(
    Guid Id,
    string DisplayName,
    UserRole Role,
    string? Group);
