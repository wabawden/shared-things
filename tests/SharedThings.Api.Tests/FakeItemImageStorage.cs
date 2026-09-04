using SharedThings.Api.Interfaces;

namespace SharedThings.Api.Tests;

public sealed class FakeItemImageStorage
    : IItemImageStorage
{
    private readonly Dictionary<string, StoredFile> _files =
        new();

    public IReadOnlyDictionary<string, StoredFile> Files =>
        _files;

    public async Task UploadAsync(
        string key,
        Stream content,
        string contentType,
        CancellationToken cancellationToken)
    {
        await using var memory = new MemoryStream();

        await content.CopyToAsync(
            memory,
            cancellationToken);

        _files[key] = new StoredFile(
            memory.ToArray(),
            contentType);
    }

    public Task DeleteAsync(
        string key,
        CancellationToken cancellationToken)
    {
        _files.Remove(key);

        return Task.CompletedTask;
    }

    public sealed record StoredFile(
        byte[] Content,
        string ContentType);
}