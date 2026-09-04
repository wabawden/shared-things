public sealed class ItemImageOptions
{
    public const string SectionName = "ItemImages";

    public required string BucketName { get; init; }

    public required string Region { get; init; }

    public required string PublicBaseUrl { get; init; }
}