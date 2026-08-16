namespace SharedThings.Api.Contracts;

public sealed record RegisterRequest(
    string Email,
    string Password,
    string DisplayName);