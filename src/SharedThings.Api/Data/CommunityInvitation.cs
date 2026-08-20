using SharedThings.Api.Data.Entities;

namespace SharedThings.Api.Data;

public sealed class CommunityInvitation
{
    private CommunityInvitation()
    {
    }

    public CommunityInvitation(
        Guid id,
        Guid communityId,
        Guid createdByUserId,
        string tokenHash,
        DateTimeOffset expiresAt)
    {
        Id = id;
        CommunityId = communityId;
        CreatedByUserId = createdByUserId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
    }

    public Guid Id { get; private set; }

    public Guid CommunityId { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }

    public Community Community { get; private set; } = null!;

    public ApplicationUser CreatedByUser { get; private set; } = null!;

    public bool IsActive(DateTimeOffset now)
    {
        return RevokedAt is null && ExpiresAt > now;
    }

    public void Revoke(DateTimeOffset now)
    {
        RevokedAt ??= now;
    }
}