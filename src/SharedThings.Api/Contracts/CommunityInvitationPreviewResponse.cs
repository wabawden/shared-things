namespace SharedThings.Api.Contracts;

public sealed record CommunityInvitationPreviewResponse(
    Guid CommunityId,
    string CommunityName,
    DateTimeOffset ExpiresAt,
    bool AlreadyMember);