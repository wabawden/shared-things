namespace SharedThings.Api.Contracts;

public sealed record CreateCommunityInvitationResponse(
    Guid CommunityId,
    string Token,
    DateTimeOffset ExpiresAt);