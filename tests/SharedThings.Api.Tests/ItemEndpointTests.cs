using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using SharedThings.Api.Authentication;
using SharedThings.Api.Contracts;
using SharedThings.Api.Data;
using Xunit;

namespace SharedThings.Api.Tests;


public sealed class ItemEndpointsTests :
    IClassFixture<SharedThingsApiFactory>,
    IAsyncLifetime
{
    private readonly SharedThingsApiFactory _factory;
    private readonly HttpClient _client;

    public ItemEndpointsTests(
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
    
    private async Task<Guid> CreateItemAsync()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/items",
            new CreateItemRequest(
                "Test item",
                "Created for a deletion test.",
                "Good"));

        response.EnsureSuccessStatusCode();

        using var json = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync());

        return json.RootElement
            .GetProperty("id")
            .GetGuid();
    }
    
    [Fact]
    public async Task CreateItem_CreatesItemForCommunityMember()
    {
        AuthenticateAs(SeedIds.Bill);

        var response = await _client.PostAsJsonAsync(
            $"/api/items",
            new
            {
                name = "Folding table",
                description = "Six-foot folding table.",
                condition = "Good"
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var item = await response.Content.ReadFromJsonAsync<Item>();

        Assert.NotNull(item);
        Assert.NotEqual(Guid.Empty, item.Id);
        Assert.Equal(SeedIds.Bill, item.OwnerId);
        Assert.Equal("Folding table", item.Name);
        Assert.Equal("Six-foot folding table.", item.Description);
        Assert.Equal("Good", item.Condition);
    }
    
    [Fact]
    public async Task CreateItem_AddsItemToCommunityCatalogue()
    {
        AuthenticateAs(SeedIds.Bill);

        var createResponse = await _client.PostAsJsonAsync(
            $"/api/items",
            new
            {
                name = "Unique garden shredder",
                description = "Suitable for small branches.",
                condition = "Well used"
            });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createdItem =
            await createResponse.Content.ReadFromJsonAsync<Item>();

        Assert.NotNull(createdItem);

        var catalogueResponse = await _client.GetAsync(
            $"/api/items?communityId={SeedIds.Neighbourhood}");

        Assert.Equal(HttpStatusCode.OK, catalogueResponse.StatusCode);

        var catalogue =
            await catalogueResponse.Content.ReadFromJsonAsync<Item[]>();

        Assert.NotNull(catalogue);
        Assert.Contains(
            catalogue,
            item => item.Id == createdItem.Id);
    }
    
    [Fact]
    public async Task CreateItem_MakesItemVisibleToOtherCommunityMembers()
    {
        AuthenticateAs(SeedIds.Bill);

        var createResponse = await _client.PostAsJsonAsync(
            $"/api/items",
            new
            {
                name = "Tile cutter",
                description = "Manual tile cutter.",
                condition = "Good"
            });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createdItem =
            await createResponse.Content.ReadFromJsonAsync<Item>();

        Assert.NotNull(createdItem);

        AuthenticateAs(SeedIds.Alex);

        var catalogueResponse = await _client.GetAsync(
            $"/api/items?communityId={SeedIds.Neighbourhood}");

        Assert.Equal(HttpStatusCode.OK, catalogueResponse.StatusCode);

        var catalogue =
            await catalogueResponse.Content.ReadFromJsonAsync<Item[]>();

        Assert.Contains(
            catalogue!,
            item => item.Id == createdItem.Id);
    }
    
    [Fact]
    public async Task CreateItem_RejectsUnauthenticatedRequest()
    {
        _client.DefaultRequestHeaders.Remove(
            DevelopmentAuthenticationHandler.UserIdHeader);

        var response = await _client.PostAsJsonAsync(
            $"/api/items",
            new
            {
                name = "Cordless drill",
                description = "18V drill.",
                condition = "Good"
            });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task CreateItem_UsesAuthenticatedUserAsOwner()
    {
        AuthenticateAs(SeedIds.Bill);

        var response = await _client.PostAsJsonAsync(
            $"/api/items",
            new
            {
                ownerId = SeedIds.Alex,
                name = "Pressure washer",
                description = "Compact pressure washer.",
                condition = "Good"
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var item = await response.Content.ReadFromJsonAsync<Item>();

        Assert.NotNull(item);
        Assert.Equal(SeedIds.Bill, item.OwnerId);
        Assert.NotEqual(SeedIds.Alex, item.OwnerId);
    }
    
    [Fact]
    public async Task CreateItem_RejectsBlankName()
    {
        AuthenticateAs(SeedIds.Bill);

        var response = await _client.PostAsJsonAsync(
            $"/api/items",
            new
            {
                name = "   ",
                description = "An item without a valid name.",
                condition = "Good"
            });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task GetItem_ReturnsItemForOwner()
    {
        AuthenticateAs(SeedIds.Bill);

        var response = await _client.GetAsync(
            $"/api/items/{SeedIds.CordlessDrill}");

        var item = await response.Content
            .ReadFromJsonAsync<ItemDetailsResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(item);
        Assert.Equal(SeedIds.CordlessDrill, item.Id);
        Assert.Equal("Cordless drill", item.Name);
        Assert.True(item.CanEdit);
    }
    
    [Fact]
    public async Task GetItem_ReturnsItemForUserWhoSharesCommunityWithOwner()
    {
        AuthenticateAs(SeedIds.Alex);

        var response = await _client.GetAsync(
            $"/api/items/{SeedIds.CordlessDrill}");

        var item = await response.Content
            .ReadFromJsonAsync<ItemDetailsResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(item);
        Assert.Equal("Cordless drill", item.Name);
        Assert.Equal(SeedIds.Bill, item.Owner.Id);
        Assert.Equal("Bill", item.Owner.DisplayName);
        Assert.False(item.CanEdit);
    }
    
    [Fact]
    public async Task GetItem_ReturnsNotFoundForUserWhoDoesNotShareCommunityWithOwner()
    {
        AuthenticateAs(SeedIds.Casey);

        var response = await _client.GetAsync(
            $"/api/items/{SeedIds.CordlessDrill}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task GetItem_ReturnsNotFoundWhenItemDoesNotExist()
    {
        AuthenticateAs(SeedIds.Bill);

        var response = await _client.GetAsync(
            $"/api/items/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task GetItem_ReturnsUnauthorizedForUnauthenticatedUser()
    {
        var response = await _client.GetAsync(
            $"/api/items/{SeedIds.CordlessDrill}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task GetItem_ReturnsItemDetails()
    {
        AuthenticateAs(SeedIds.Bill);

        var item = await _client.GetFromJsonAsync<ItemDetailsResponse>(
            $"/api/items/{SeedIds.CordlessDrill}");

        Assert.NotNull(item);
        Assert.Equal("Cordless drill", item.Name);
        Assert.Equal(
            "18V drill with charger and a small set of bits.",
            item.Description);
        Assert.Equal("Good", item.Condition);
        Assert.Equal(SeedIds.Bill, item.Owner.Id);
        Assert.Equal("Bill", item.Owner.DisplayName);
    }
    
    [Fact]
    public async Task UpdateItem_UpdatesAllFieldsForOwner()
    {
        AuthenticateAs(SeedIds.Bill);

        var request = new UpdateItemRequest(
            "Updated drill",
            "Updated description",
            "Excellent");

        var response = await _client.PutAsJsonAsync(
            $"/api/items/{SeedIds.CordlessDrill}",
            request);

        var item = await response.Content
            .ReadFromJsonAsync<ItemDetailsResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(item);
        Assert.Equal("Updated drill", item.Name);
        Assert.Equal("Updated description", item.Description);
        Assert.Equal("Excellent", item.Condition);
        Assert.Equal(SeedIds.Bill, item.Owner.Id);
        Assert.True(item.CanEdit);
    }
    
    [Fact]
    public async Task UpdateItem_PersistsChanges()
    {
        AuthenticateAs(SeedIds.Bill);

        var request = new UpdateItemRequest(
            "Updated drill",
            "Now includes two batteries.",
            "Very good");

        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/items/{SeedIds.CordlessDrill}",
            request);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var item = await _client.GetFromJsonAsync<ItemDetailsResponse>(
            $"/api/items/{SeedIds.CordlessDrill}");

        Assert.NotNull(item);
        Assert.Equal("Updated drill", item.Name);
        Assert.Equal("Now includes two batteries.", item.Description);
        Assert.Equal("Very good", item.Condition);
    }
    
    [Fact]
    public async Task UpdateItem_ReturnsNotFoundForNonOwner()
    {
        AuthenticateAs(SeedIds.Alex);

        var request = new UpdateItemRequest(
            "Alex changed this",
            "This must not be saved.",
            "Broken");

        var response = await _client.PutAsJsonAsync(
            $"/api/items/{SeedIds.CordlessDrill}",
            request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task UpdateItem_DoesNotChangeItemWhenUserIsNotOwner()
    {
        AuthenticateAs(SeedIds.Alex);

        var request = new UpdateItemRequest(
            "Alex changed this",
            "This must not be saved.",
            "Broken");

        await _client.PutAsJsonAsync(
            $"/api/items/{SeedIds.CordlessDrill}",
            request);

        AuthenticateAs(SeedIds.Bill);

        var item = await _client.GetFromJsonAsync<ItemDetailsResponse>(
            $"/api/items/{SeedIds.CordlessDrill}");

        Assert.NotNull(item);
        Assert.Equal("Cordless drill", item.Name);
        Assert.Equal("Good", item.Condition);
    }
    
    [Fact]
    public async Task UpdateItem_ReturnsNotFoundWhenItemDoesNotExist()
    {
        AuthenticateAs(SeedIds.Bill);

        var request = new UpdateItemRequest(
            "Updated name",
            "Updated description",
            "Good");

        var response = await _client.PutAsJsonAsync(
            $"/api/items/{Guid.NewGuid()}",
            request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task UpdateItem_ReturnsValidationProblemWhenNameIsEmpty()
    {
        AuthenticateAs(SeedIds.Bill);

        var request = new UpdateItemRequest(
            "   ",
            "Description",
            "Good");

        var response = await _client.PutAsJsonAsync(
            $"/api/items/{SeedIds.CordlessDrill}",
            request);

        var problem = await response.Content
            .ReadFromJsonAsync<HttpValidationProblemDetails>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Contains("name", problem.Errors.Keys);
    }
    
    [Theory]
    [InlineData(101, 10, 10, "name")]
    [InlineData(10, 256, 10, "description")]
    [InlineData(10, 10, 101, "condition")]
    public async Task UpdateItem_ReturnsValidationProblemForFieldsThatAreTooLong(
        int nameLength,
        int descriptionLength,
        int conditionLength,
        string expectedField)
    {
        AuthenticateAs(SeedIds.Bill);

        var request = new UpdateItemRequest(
            new string('n', nameLength),
            new string('d', descriptionLength),
            new string('c', conditionLength));

        var response = await _client.PutAsJsonAsync(
            $"/api/items/{SeedIds.CordlessDrill}",
            request);

        var problem = await response.Content
            .ReadFromJsonAsync<HttpValidationProblemDetails>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Contains(expectedField, problem.Errors.Keys);
    }
    
    [Fact]
    public async Task UpdateItem_ReturnsUnauthorizedForUnauthenticatedUser()
    {
        var request = new UpdateItemRequest(
            "Updated drill",
            "Updated description",
            "Excellent");

        var response = await _client.PutAsJsonAsync(
            $"/api/items/{SeedIds.CordlessDrill}",
            request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task DeleteItem_RemovesItemOwnedByCurrentUser()
    {
        AuthenticateAs(SeedIds.Bill);

        var itemId = await CreateItemAsync();

        var deleteResponse = await _client.DeleteAsync(
            $"/api/items/{itemId}");

        Assert.Equal(
            HttpStatusCode.NoContent,
            deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync(
            $"/api/items/{itemId}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            getResponse.StatusCode);
    }
    
    [Fact]
    public async Task DeleteItem_ReturnsNotFoundWhenItemDoesNotExist()
    {
        AuthenticateAs(SeedIds.Bill);

        var missingItemId = Guid.NewGuid();

        var response = await _client.DeleteAsync(
            $"/api/items/{missingItemId}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }
    
    [Fact]
    public async Task DeleteItem_DoesNotAllowAnotherUserToDeleteItem()
    {
        AuthenticateAs(SeedIds.Bill);

        var itemId = await CreateItemAsync();

        AuthenticateAs(SeedIds.Alex);

        var deleteResponse = await _client.DeleteAsync(
            $"/api/items/{itemId}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            deleteResponse.StatusCode);

        AuthenticateAs(SeedIds.Bill);

        var getResponse = await _client.GetAsync(
            $"/api/items/{itemId}");

        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode);
    }
    
    [Fact]
    public async Task DeleteItem_RequiresAuthentication()
    {
        var itemId = Guid.NewGuid();

        var response = await _client.DeleteAsync(
            $"/api/items/{itemId}");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }
    
    private void AuthenticateAs(Guid userId)
    {
        _client.DefaultRequestHeaders.Remove(DevelopmentAuthenticationHandler.UserIdHeader);
        _client.DefaultRequestHeaders.Add(
            DevelopmentAuthenticationHandler.UserIdHeader,
            userId.ToString());
    }
}