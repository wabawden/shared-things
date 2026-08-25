namespace SharedThings.Api.Contracts;

public sealed record ItemOwnerResponse(
    Guid Id,
    string DisplayName);