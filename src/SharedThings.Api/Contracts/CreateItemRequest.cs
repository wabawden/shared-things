namespace SharedThings.Api.Contracts;

public sealed record CreateItemRequest(string Name, string? Description, string? Condition);