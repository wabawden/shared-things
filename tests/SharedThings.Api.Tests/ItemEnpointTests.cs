using System.Net;
using System.Net.Http.Json;
using SharedThings.Api.Authentication;
using SharedThings.Api.Data;
using Xunit;

namespace SharedThings.Api.Tests;


public sealed class ItemEndpointsTests : IClassFixture<SharedThingsApiFactory>
{
    private readonly HttpClient _client;

    public ItemEndpointsTests(SharedThingsApiFactory application)
    {
        _client = application.CreateClient();
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
    
    private void AuthenticateAs(Guid userId)
    {
        _client.DefaultRequestHeaders.Remove(DevelopmentAuthenticationHandler.UserIdHeader);
        _client.DefaultRequestHeaders.Add(
            DevelopmentAuthenticationHandler.UserIdHeader,
            userId.ToString());
    }
}