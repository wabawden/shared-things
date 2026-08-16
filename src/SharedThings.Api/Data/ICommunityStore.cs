using SharedThings.Api.Data.Entities;

namespace SharedThings.Api.Data;

public interface ICommunityStore
{
    IReadOnlyCollection<Community> GetCommunitiesFor(Guid userId);
    IReadOnlyCollection<Item> GetCommunityItems(Guid communityId);
    IReadOnlyCollection<Item> GetMyItems(Guid userId);
    bool IsMember(Guid userId, Guid communityId);
    Community CreateCommunity(Guid creatorId, string name);
    Item CreateItem(
        Guid creatorId,
        string name,
        string? description,
        string? condition);
}
