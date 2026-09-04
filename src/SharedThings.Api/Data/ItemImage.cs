namespace SharedThings.Api.Data;

public class ItemImage
{
    public Guid Id { get; set; }

    public Guid ItemId { get; set; }

    public Item Item { get; set; } = null!;
    
    public string StorageKey { get; set; } = null!;

    public string ContentType { get; set; } = null!;

    public int SortOrder { get; set; }

    public DateTimeOffset UploadedAt { get; set; }

}