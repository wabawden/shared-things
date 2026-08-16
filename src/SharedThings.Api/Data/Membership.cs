using SharedThings.Api.Data.Entities;

namespace SharedThings.Api.Data;

public sealed class Membership
{
    private Membership()
    {
    }

    public Membership(Guid userId, Guid communityId)
    {
        UserId = userId;
        CommunityId = communityId;
    }
    
    public Guid UserId { get; set; }
    public Guid CommunityId { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public Community Community { get; set; } = null!;
}