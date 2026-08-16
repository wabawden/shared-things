using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using SharedThings.Api.Authentication;
using SharedThings.Api.Data;
using SharedThings.Api.Data.Entities;
using Xunit;

namespace SharedThings.Api.Tests;

public sealed class CommunityEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public CommunityEndpointsTests(WebApplicationFactory<Program> application)
    {
        _client = application.CreateClient();
    }

    [Fact]
    public async Task MyCommunities_ReturnsOnlyCommunitiesTheUserBelongsTo()
    {
        AuthenticateAs(SeedIds.Bill);

        var communities = await _client.GetFromJsonAsync<Community[]>("/api/communities");

        var community = Assert.Single(communities!);
        Assert.Equal(SeedIds.Neighbourhood, community.Id);
    }

    [Fact]
    public async Task CommunityItems_ReturnsCatalogueForMember()
    {
        AuthenticateAs(SeedIds.Alex);

        var response = await _client.GetAsync(
            $"/api/items?communityId={SeedIds.Neighbourhood}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var items = await response.Content.ReadFromJsonAsync<Item[]>();

        Assert.NotNull(items);
        Assert.Equal(2, items.Length);
    }

    [Fact]
    public async Task CommunityItems_DoesNotRevealCatalogueToNonMember()
    {
        AuthenticateAs(SeedIds.Casey);

        var response = await _client.GetAsync(
            $"/api/items?communityId={SeedIds.Neighbourhood}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Endpoints_RejectUnauthenticatedRequests()
    {
        var response = await _client.GetAsync("/api/communities");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task CreateCommunity_CreatesCommunityAndAddsCreatorAsMember()
    {
        AuthenticateAs(SeedIds.Casey);

        var response = await _client.PostAsJsonAsync(
            "/api/communities",
            new
            {
                name = "Malmesbury Parents"
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdCommunity =
            await response.Content.ReadFromJsonAsync<Community>();

        Assert.NotNull(createdCommunity);
        Assert.Equal("Malmesbury Parents", createdCommunity.Name);

        var communities = await _client.GetFromJsonAsync<Community[]>(
            "/api/communities");

        Assert.Contains(
            communities!,
            community => community.Id == createdCommunity.Id);
    }

    [Fact]
    public async Task CreateCommunity_RejectsBlankName()
    {
        AuthenticateAs(SeedIds.Casey);

        var response = await _client.PostAsJsonAsync(
            "/api/communities",
            new
            {
                name = "   "
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
