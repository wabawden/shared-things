using SharedThings.Api.Contracts;

public sealed record ItemDetailsResponse(
    Guid Id,
    string Name,
    string Description,
    string Condition,
    ItemOwnerResponse Owner,
    bool CanEdit);