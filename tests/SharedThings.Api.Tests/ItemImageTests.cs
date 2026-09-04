using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedThings.Api.Authentication;
using SharedThings.Api.Data;
using Xunit;

namespace SharedThings.Api.Tests;


public sealed class ItemImageTests : 
    IClassFixture<SharedThingsApiFactory>, IAsyncLifetime
{
    private readonly SharedThingsApiFactory _factory;
    private readonly HttpClient _client;

    public ItemImageTests(
        SharedThingsApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }
    
    public Task InitializeAsync()
    {
        return _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
    
    private static byte[] CreateTestPng()
    {
        return Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAAB" +
            "CAQAAAC1HAwCAAAAC0lEQVR42mNk+A8A" +
            "AQUBAScY42YAAAAASUVORK5CYII=");
    }
    
    private static byte[] CreateFirstTestPng()
    {
        return Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAAB" +
            "CAQAAAC1HAwCAAAAC0lEQVR42mNk+A8A" +
            "AQUBAScY42YAAAAASUVORK5CYII=");
    }

    private static byte[] CreateSecondTestPng()
    {
        return Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAAB" +
            "CAQAAAC1HAwCAAAAC0lEQVR42mP8/x8A" +
            "AusB9Y9Z4WQAAAAASUVORK5CYII=");
    }
    
    private const int MaximumImageSize =
        5 * 1024 * 1024;
    
    private void AuthenticateAs(Guid userId)
    {
        _client.DefaultRequestHeaders.Remove(DevelopmentAuthenticationHandler.UserIdHeader);
        _client.DefaultRequestHeaders.Add(
            DevelopmentAuthenticationHandler.UserIdHeader,
            userId.ToString());
    }
    
    private async Task<ItemImage?> GetItemImageAsync(
        Guid itemId)
    {
        await using var scope =
            _factory.Services.CreateAsyncScope();

        var db = scope.ServiceProvider
            .GetRequiredService<SharedThingsDbContext>();

        return await db.ItemImages
            .AsNoTracking()
            .SingleOrDefaultAsync(
                image => image.ItemId == itemId);
    }
    
    private async Task<List<ItemImage>> GetItemImagesAsync(
        Guid itemId)
    {
        await using var scope =
            _factory.Services.CreateAsyncScope();

        var db = scope.ServiceProvider
            .GetRequiredService<SharedThingsDbContext>();

        return await db.ItemImages
            .AsNoTracking()
            .Where(image => image.ItemId == itemId)
            .OrderBy(image => image.SortOrder)
            .ToListAsync();
    }
    
    [Fact]
    public async Task UploadItemImage_StoresImageForOwnedItem()
    {
        AuthenticateAs(SeedIds.Bill);

        var imageBytes = CreateTestPng();

        using var content = new MultipartFormDataContent();

        using var imageContent =
            new ByteArrayContent(imageBytes);

        imageContent.Headers.ContentType =
            new MediaTypeHeaderValue("image/png");

        content.Add(
            imageContent,
            "image",
            "test-image.png");

        var response = await _client.PutAsync(
            $"/api/items/{SeedIds.CordlessDrill}/image",
            content);

        Assert.Equal(
            HttpStatusCode.NoContent,
            response.StatusCode);

        var storedImage = await GetItemImageAsync(
            SeedIds.CordlessDrill);

        Assert.NotNull(storedImage);
        Assert.Equal(
            "image/png",
            storedImage.ContentType);
        Assert.Equal(0, storedImage.SortOrder);

        Assert.True(
            _factory.ImageStorage.Files.ContainsKey(
                storedImage.StorageKey));

        var uploadedFile =
            _factory.ImageStorage.Files[
                storedImage.StorageKey
            ];

        Assert.Equal("image/png", uploadedFile.ContentType);
        Assert.Equal(imageBytes, uploadedFile.Content);
    }

    [Fact]
public async Task UploadItemImage_ReplacesExistingImage()
{
    AuthenticateAs(SeedIds.Bill);

    var firstImageBytes = CreateFirstTestPng();

    using var firstContent =
        new MultipartFormDataContent();

    using var firstImageContent =
        new ByteArrayContent(firstImageBytes);

    firstImageContent.Headers.ContentType =
        new MediaTypeHeaderValue("image/png");

    firstContent.Add(
        firstImageContent,
        "image",
        "first-image.png");

    var firstResponse = await _client.PutAsync(
        $"/api/items/{SeedIds.CordlessDrill}/image",
        firstContent);

    Assert.Equal(
        HttpStatusCode.NoContent,
        firstResponse.StatusCode);

    var firstImage = await GetItemImageAsync(
        SeedIds.CordlessDrill);

    Assert.NotNull(firstImage);

    var firstStorageKey = firstImage.StorageKey;

    Assert.True(
        _factory.ImageStorage.Files.ContainsKey(
            firstStorageKey));

    var replacementBytes = CreateSecondTestPng();

    using var replacementContent =
        new MultipartFormDataContent();

    using var replacementImageContent =
        new ByteArrayContent(replacementBytes);

    replacementImageContent.Headers.ContentType =
        new MediaTypeHeaderValue("image/png");

    replacementContent.Add(
        replacementImageContent,
        "image",
        "replacement-image.png");

    var replacementResponse = await _client.PutAsync(
        $"/api/items/{SeedIds.CordlessDrill}/image",
        replacementContent);

    Assert.Equal(
        HttpStatusCode.NoContent,
        replacementResponse.StatusCode);

    var storedImages = await GetItemImagesAsync(
        SeedIds.CordlessDrill);

    var replacementImage = Assert.Single(storedImages);

    Assert.NotEqual(
        firstStorageKey,
        replacementImage.StorageKey);

    Assert.Equal(
        "image/png",
        replacementImage.ContentType);

    Assert.Equal(
        0,
        replacementImage.SortOrder);

    Assert.False(
        _factory.ImageStorage.Files.ContainsKey(
            firstStorageKey));

    Assert.True(
        _factory.ImageStorage.Files.ContainsKey(
            replacementImage.StorageKey));

    var storedReplacement =
        _factory.ImageStorage.Files[
            replacementImage.StorageKey
        ];

    Assert.Equal(
        "image/png",
        storedReplacement.ContentType);

    Assert.Equal(
        replacementBytes,
        storedReplacement.Content);
}

    [Fact]
    public async Task DeleteItemImage_RemovesImageButPreservesItem()
    {
        AuthenticateAs(SeedIds.Bill);

        var imageBytes = CreateTestPng();

        using var uploadContent =
            new MultipartFormDataContent();

        using var imageContent =
            new ByteArrayContent(imageBytes);

        imageContent.Headers.ContentType =
            new MediaTypeHeaderValue("image/png");

        uploadContent.Add(
            imageContent,
            "image",
            "test-image.png");

        var uploadResponse = await _client.PutAsync(
            $"/api/items/{SeedIds.CordlessDrill}/image",
            uploadContent);

        Assert.Equal(
            HttpStatusCode.NoContent,
            uploadResponse.StatusCode);

        var imageBeforeDeletion = await GetItemImageAsync(
            SeedIds.CordlessDrill);

        Assert.NotNull(imageBeforeDeletion);

        var storageKey =
            imageBeforeDeletion.StorageKey;

        Assert.True(
            _factory.ImageStorage.Files.ContainsKey(
                storageKey));

        var deleteResponse = await _client.DeleteAsync(
            $"/api/items/{SeedIds.CordlessDrill}/image");

        Assert.Equal(
            HttpStatusCode.NoContent,
            deleteResponse.StatusCode);

        var imageAfterDeletion = await GetItemImageAsync(
            SeedIds.CordlessDrill);

        Assert.Null(imageAfterDeletion);

        Assert.False(
            _factory.ImageStorage.Files.ContainsKey(
                storageKey));

        var itemResponse = await _client.GetAsync(
            $"/api/items/{SeedIds.CordlessDrill}");

        Assert.Equal(
            HttpStatusCode.OK,
            itemResponse.StatusCode);
    }
    
    [Fact]
    public async Task DeleteItemImage_ReturnsNoContentWhenItemHasNoImage()
    {
        AuthenticateAs(SeedIds.Bill);

        var response = await _client.DeleteAsync(
            $"/api/items/{SeedIds.CordlessDrill}/image");

        Assert.Equal(
            HttpStatusCode.NoContent,
            response.StatusCode);

        var storedImage = await GetItemImageAsync(
            SeedIds.CordlessDrill);

        Assert.Null(storedImage);
    }
    
    [Fact]
    public async Task DeleteItemImage_ReturnsNotFoundForItemOwnedByAnotherUser()
    {
        AuthenticateAs(SeedIds.Bill);

        var imageBytes = CreateTestPng();

        using var uploadContent =
            new MultipartFormDataContent();

        using var imageContent =
            new ByteArrayContent(imageBytes);

        imageContent.Headers.ContentType =
            new MediaTypeHeaderValue("image/png");

        uploadContent.Add(
            imageContent,
            "image",
            "test-image.png");

        var uploadResponse = await _client.PutAsync(
            $"/api/items/{SeedIds.CordlessDrill}/image",
            uploadContent);

        Assert.Equal(
            HttpStatusCode.NoContent,
            uploadResponse.StatusCode);

        var storedImage = await GetItemImageAsync(
            SeedIds.CordlessDrill);

        Assert.NotNull(storedImage);

        var storageKey = storedImage.StorageKey;

        AuthenticateAs(SeedIds.Alex);

        var deleteResponse = await _client.DeleteAsync(
            $"/api/items/{SeedIds.CordlessDrill}/image");

        Assert.Equal(
            HttpStatusCode.NotFound,
            deleteResponse.StatusCode);

        var imageAfterRequest = await GetItemImageAsync(
            SeedIds.CordlessDrill);

        Assert.NotNull(imageAfterRequest);

        Assert.Equal(
            storageKey,
            imageAfterRequest.StorageKey);

        Assert.True(
            _factory.ImageStorage.Files.ContainsKey(
                storageKey));
    }
    
    [Fact]
    public async Task DeleteItemImage_ReturnsNotFoundWhenItemDoesNotExist()
    {
        AuthenticateAs(SeedIds.Bill);

        var response = await _client.DeleteAsync(
            $"/api/items/{Guid.NewGuid()}/image");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }
    
    [Fact]
    public async Task DeleteItemImage_RequiresAuthentication()
    {
        var response = await _client.DeleteAsync(
            $"/api/items/{SeedIds.CordlessDrill}/image");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }
    
    [Fact]
    public async Task UploadItemImage_ReturnsBadRequestForEmptyFile()
    {
        AuthenticateAs(SeedIds.Bill);

        using var content =
            new MultipartFormDataContent();

        using var imageContent =
            new ByteArrayContent([]);

        imageContent.Headers.ContentType =
            new MediaTypeHeaderValue("image/png");

        content.Add(
            imageContent,
            "image",
            "empty.png");

        var response = await _client.PutAsync(
            $"/api/items/{SeedIds.CordlessDrill}/image",
            content);

        var problem = await response.Content
            .ReadFromJsonAsync<HttpValidationProblemDetails>();

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        Assert.NotNull(problem);
        Assert.Contains("image", problem.Errors.Keys);

        Assert.Null(
            await GetItemImageAsync(
                SeedIds.CordlessDrill));
    }
    
    [Fact]
    public async Task UploadItemImage_ReturnsBadRequestForOversizedFile()
    {
        AuthenticateAs(SeedIds.Bill);

        var imageBytes =
            new byte[MaximumImageSize + 1];

        using var content =
            new MultipartFormDataContent();

        using var imageContent =
            new ByteArrayContent(imageBytes);

        imageContent.Headers.ContentType =
            new MediaTypeHeaderValue("image/png");

        content.Add(
            imageContent,
            "image",
            "oversized.png");

        var response = await _client.PutAsync(
            $"/api/items/{SeedIds.CordlessDrill}/image",
            content);

        var problem = await response.Content
            .ReadFromJsonAsync<HttpValidationProblemDetails>();

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        Assert.NotNull(problem);
        Assert.Contains("image", problem.Errors.Keys);

        Assert.Null(
            await GetItemImageAsync(
                SeedIds.CordlessDrill));
    }
    
    [Fact]
    public async Task UploadItemImage_ReturnsBadRequestForUnsupportedContentType()
    {
        AuthenticateAs(SeedIds.Bill);

        var imageBytes = Encoding.UTF8.GetBytes(
            "<svg xmlns=\"http://www.w3.org/2000/svg\"></svg>");

        using var content =
            new MultipartFormDataContent();

        using var imageContent =
            new ByteArrayContent(imageBytes);

        imageContent.Headers.ContentType =
            new MediaTypeHeaderValue("image/svg+xml");

        content.Add(
            imageContent,
            "image",
            "image.svg");

        var response = await _client.PutAsync(
            $"/api/items/{SeedIds.CordlessDrill}/image",
            content);

        var problem = await response.Content
            .ReadFromJsonAsync<HttpValidationProblemDetails>();

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        Assert.NotNull(problem);
        Assert.Contains("image", problem.Errors.Keys);

        Assert.Null(
            await GetItemImageAsync(
                SeedIds.CordlessDrill));
    }
    
    [Fact]
    public async Task UploadItemImage_ReturnsBadRequestWhenContentIsNotAnImage()
    {
        AuthenticateAs(SeedIds.Bill);

        var imageBytes = Encoding.UTF8.GetBytes(
            "This is not really a PNG.");

        using var content =
            new MultipartFormDataContent();

        using var imageContent =
            new ByteArrayContent(imageBytes);

        imageContent.Headers.ContentType =
            new MediaTypeHeaderValue("image/png");

        content.Add(
            imageContent,
            "image",
            "not-really-an-image.png");

        var response = await _client.PutAsync(
            $"/api/items/{SeedIds.CordlessDrill}/image",
            content);

        var problem = await response.Content
            .ReadFromJsonAsync<HttpValidationProblemDetails>();

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        Assert.NotNull(problem);
        Assert.Contains("image", problem.Errors.Keys);

        Assert.Null(
            await GetItemImageAsync(
                SeedIds.CordlessDrill));
    }
    
    [Fact]
    public async Task UploadItemImage_ReturnsBadRequestWhenContentTypeDoesNotMatchFile()
    {
        AuthenticateAs(SeedIds.Bill);

        var imageBytes = CreateTestPng();

        using var content =
            new MultipartFormDataContent();

        using var imageContent =
            new ByteArrayContent(imageBytes);

        imageContent.Headers.ContentType =
            new MediaTypeHeaderValue("image/jpeg");

        content.Add(
            imageContent,
            "image",
            "pretending-to-be-jpeg.jpg");

        var response = await _client.PutAsync(
            $"/api/items/{SeedIds.CordlessDrill}/image",
            content);

        var problem = await response.Content
            .ReadFromJsonAsync<HttpValidationProblemDetails>();

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        Assert.NotNull(problem);
        Assert.Contains("image", problem.Errors.Keys);

        Assert.Null(
            await GetItemImageAsync(
                SeedIds.CordlessDrill));
    }
    
    
    
}