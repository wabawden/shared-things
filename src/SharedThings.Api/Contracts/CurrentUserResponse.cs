namespace SharedThings.Api.Contracts;

public sealed record CurrentUserResponse(
    Guid Id,
    string Email,
    string DisplayName);