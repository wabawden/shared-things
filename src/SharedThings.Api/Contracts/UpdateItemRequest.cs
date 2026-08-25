namespace SharedThings.Api.Contracts;

public sealed record UpdateItemRequest(
    string Name,
    string? Description,
    string? Condition);