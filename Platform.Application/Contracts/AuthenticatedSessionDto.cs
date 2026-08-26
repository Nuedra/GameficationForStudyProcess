namespace Platform.Application.Contracts;

public sealed record AuthenticatedSessionDto(
    AuthenticatedUserDto User,
    Guid SessionId,
    DateTimeOffset? IssuedUtc,
    DateTimeOffset? ExpiresUtc);
