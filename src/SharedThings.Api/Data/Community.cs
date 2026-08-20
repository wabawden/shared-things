using SharedThings.Api.Data.Entities;

namespace SharedThings.Api.Data;

public sealed class Community
{
    private Community()
    {
    }

    public Community(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public ICollection<Membership> Memberships { get; private set; } = [];
    
    public ICollection<CommunityInvitation> Invitations { get; private set; } = [];
}