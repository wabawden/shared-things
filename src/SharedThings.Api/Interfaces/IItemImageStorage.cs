namespace SharedThings.Api.Interfaces;

public interface IItemImageStorage
{
    Task UploadAsync(
        string key,
        Stream content,
        string contentType,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        string key,
        CancellationToken cancellationToken);
}