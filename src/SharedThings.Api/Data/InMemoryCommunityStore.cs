using SharedThings.Api.Data.Entities;

namespace SharedThings.Api.Data;

public sealed class InMemoryCommunityStore : ICommunityStore
{
    private readonly object _lock = new();

    private readonly Dictionary<Guid, Community> _communities = new()
    {
        [SeedIds.Neighbourhood] =
            new Community(SeedIds.Neighbourhood, "Our Neighbourhood")
    };

    private readonly Dictionary<Guid, HashSet<Guid>> _memberships = new()
    {
        [SeedIds.Neighbourhood] =
            [SeedIds.Bill, SeedIds.Alex]
    };

    private static readonly IReadOnlySet<Guid> UserIds =
        new HashSet<Guid>
        {
            SeedIds.Bill,
            SeedIds.Alex,
            SeedIds.Casey
        };

    private readonly Dictionary<Guid, Item> _items =
        new()
        {
            [Guid.Parse("30000000-0000-0000-0000-000000000001")] =
                new(
                    Guid.Parse("30000000-0000-0000-0000-000000000001"),
                    SeedIds.Bill,
                    "Cordless drill",
                    "18V drill with charger and a small set of bits.",
                    "Good"),
            [Guid.Parse("30000000-0000-0000-0000-000000000002")] =
                new(
                    Guid.Parse("30000000-0000-0000-0000-000000000002"),
                    SeedIds.Alex,
                    "Wallpaper steamer",
                    "Compact wallpaper steamer. Please let it cool before returning.",
                    "Well used")
        };

    public IReadOnlyCollection<Community> GetCommunitiesFor(Guid userId)
    {
        lock (_lock)
        {
            return _memberships
                .Where(pair => pair.Value.Contains(userId))
                .Select(pair => _communities[pair.Key])
                .ToArray();
        }
    }

    public IReadOnlyCollection<Item> GetCommunityItems(Guid communityId)
    {
        lock (_lock)
        {
            if (!_memberships.TryGetValue(communityId, out var members))
            {
                return Array.Empty<Item>();
            }

            return _items.Values
                .Where(item => members.Contains(item.OwnerId))
                .ToArray();
        }
    }
    
    public IReadOnlyCollection<Item> GetMyItems(Guid userId)
    {
        lock (_lock)
        {
            return _items
                .Where(item => userId == item.Value.OwnerId)
                .Select(i =>  i.Value)
                .ToArray<Item>();
        }
    }

    public bool IsMember(Guid userId, Guid communityId)
    {
        lock (_lock)
        {
            return _memberships.TryGetValue(communityId, out var members) && members.Contains(userId);
        }
    }

    public Community CreateCommunity(Guid creatorId, string name)
    {
        var community = new Community(
            Guid.NewGuid(),
            name);

        lock (_lock)
        {
            _communities.Add(community.Id, community);
            _memberships.Add(
                community.Id,
                new HashSet<Guid> { creatorId });
        }

        return community;
    }

    public Item CreateItem(
        Guid creatorId,
        string name,
        string? description,
        string? condition)
    {
        lock (_lock)
        {
            if (!UserIds.Contains(creatorId))
            {
                throw new ArgumentException(
                    "The creator does not exist.",
                    nameof(creatorId));
            }

            var item = new Item(
                Guid.NewGuid(),
                creatorId,
                name,
                description ?? string.Empty,
                condition ?? string.Empty);
            
            _items.Add(item.Id, item);
            return item;
        }
    }
}
