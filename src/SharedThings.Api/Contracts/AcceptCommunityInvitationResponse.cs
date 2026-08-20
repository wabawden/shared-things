namespace SharedThings.Api.Contracts;

public sealed record AcceptCommunityInvitationResponse(
    Guid CommunityId,
    string CommunityName,
    bool MembershipCreated);