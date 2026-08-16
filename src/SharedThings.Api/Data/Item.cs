using SharedThings.Api.Data.Entities;

namespace SharedThings.Api.Data;

public sealed class Item
{
    private Item()
    {
    }

    public Item(
        Guid id,
        Guid ownerId,
        string name,
        string description,
        string condition)
    {
        Id = id;
        OwnerId = ownerId;
        Name = name;
        Description = description;
        Condition = condition;
    }

    public Guid Id { get; private set; }

    public Guid OwnerId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public string Condition { get; private set; } = string.Empty;

    public ApplicationUser Owner { get; private set; } = null!;
}