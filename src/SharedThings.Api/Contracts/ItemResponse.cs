namespace SharedThings.Api.Contracts;

public sealed record ItemResponse(
    Guid Id,
    Guid OwnerId,
    string OwnerDisplayName,
    string Name,
    string Description,
    string Condition);