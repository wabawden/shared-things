using System.Net;
using System.Net.Http.Json;
using SharedThings.Api.Authentication;
using SharedThings.Api.Contracts;
using SharedThings.Api.Data;
using Xunit;

namespace SharedThings.Api.Tests;

public sealed class CommunityEndpointsTests :
    IClassFixture<SharedThingsApiFactory>
{
    private readonly HttpClient _client;

    public CommunityEndpointsTests(
        SharedThingsApiFactory application)
    {
        _client = application.CreateClient();
    }

    [Fact]
    public async Task MyCommunities_ReturnsOnlyCommunitiesTheUserBelongsTo()
    {
        AuthenticateAs(SeedIds.Casey);

        var createResponse = await _client.PostAsJsonAsync(
            "/api/communities",
            new CreateCommunityRequest("Casey's Community"));

        var caseysCommunity =
            await createResponse.Content
                .ReadFromJsonAsync<CommunityResponse>();

        AuthenticateAs(SeedIds.Bill);

        var communities =
            await _client.GetFromJsonAsync<CommunityResponse[]>(
                "/api/communities");

        Assert.Contains(
            communities!,
            community => community.Id == SeedIds.Neighbourhood);

        Assert.DoesNotContain(
            communities!,
            community => community.Id == caseysCommunity!.Id);
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
    public async Task CreateCommunity_ReturnsCreatedCommunity()
    {
        AuthenticateAs(SeedIds.Bill);

        var response = await _client.PostAsJsonAsync(
            "/api/communities",
            new CreateCommunityRequest("Repair Café"));

        var community =
            await response.Content.ReadFromJsonAsync<CommunityResponse>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(community);
        Assert.NotEqual(Guid.Empty, community.Id);
        Assert.Equal("Repair Café", community.Name);
        Assert.Equal(
            $"/api/communities/{community.Id}",
            response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task CreateCommunity_DoesNotAddOtherUsersAsMembers()
    {
        AuthenticateAs(SeedIds.Bill);

        var createResponse = await _client.PostAsJsonAsync(
            "/api/communities",
            new CreateCommunityRequest("Repair Café"));

        var created =
            await createResponse.Content.ReadFromJsonAsync<CommunityResponse>();

        AuthenticateAs(SeedIds.Alex);

        var getResponse =
            await _client.GetAsync("/api/communities");

        var communities =
            await getResponse.Content
                .ReadFromJsonAsync<CommunityResponse[]>();

        Assert.DoesNotContain(
            communities!,
            community => community.Id == created!.Id);
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
    
    [Fact]
    public async Task GetCommunities_ReturnsCommunitiesForAuthenticatedUser()
    {
        AuthenticateAs(SeedIds.Bill);

        var response = await _client.GetAsync("/api/communities");
        var communities =
            await response.Content.ReadFromJsonAsync<CommunityResponse[]>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(
            communities!,
            community => community.Id == SeedIds.Neighbourhood);
    }

    [Fact]
    public async Task GetCommunities_DoesNotReturnCommunitiesForNonMember()
    {
        AuthenticateAs(SeedIds.Casey);

        var response = await _client.GetAsync("/api/communities");
        var communities =
            await response.Content.ReadFromJsonAsync<CommunityResponse[]>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(communities!);
    }
    
    [Fact]
    public async Task GetCommunity_ReturnsCommunityForMember()
    {
        AuthenticateAs(SeedIds.Bill);

        var response = await _client.GetAsync(
            $"/api/communities/{SeedIds.Neighbourhood}");

        var community = await response.Content
            .ReadFromJsonAsync<CommunityResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(community);
        Assert.Equal(SeedIds.Neighbourhood, community.Id);
        Assert.Equal("Our Neighbourhood", community.Name);
    }
    
    [Fact]
    public async Task GetCommunity_DoesNotRevealCommunityToNonMember()
    {
        AuthenticateAs(SeedIds.Casey);

        var response = await _client.GetAsync(
            $"/api/communities/{SeedIds.Neighbourhood}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task GetCommunity_ReturnsNotFoundForUnknownCommunity()
    {
        AuthenticateAs(SeedIds.Bill);

        var response = await _client.GetAsync(
            $"/api/communities/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task GetCommunity_ReturnsNewlyCreatedCommunityToCreator()
    {
        AuthenticateAs(SeedIds.Bill);

        var createResponse = await _client.PostAsJsonAsync(
            "/api/communities",
            new CreateCommunityRequest("Repair Café"));

        var created = await createResponse.Content
            .ReadFromJsonAsync<CommunityResponse>();

        var getResponse = await _client.GetAsync(
            $"/api/communities/{created!.Id}");

        var retrieved = await getResponse.Content
            .ReadFromJsonAsync<CommunityResponse>();

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.Equal(created, retrieved);
    }
}
