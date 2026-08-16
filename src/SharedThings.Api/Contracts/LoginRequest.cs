namespace SharedThings.Api.Contracts;

public sealed record LoginRequest(
    string Email,
    string Password,
    bool RememberMe = false);