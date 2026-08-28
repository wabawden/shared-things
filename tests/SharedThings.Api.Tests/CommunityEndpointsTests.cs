using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedThings.Api.Authentication;
using SharedThings.Api.Contracts;
using SharedThings.Api.Data;
using Xunit;

namespace SharedThings.Api.Tests;

public sealed class CommunityEndpointsTests :
    IClassFixture<SharedThingsApiFactory>,
    IAsyncLifetime
{
    private readonly SharedThingsApiFactory _factory;
    private readonly HttpClient _client;

    public CommunityEndpointsTests(
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

    private async Task<bool> CommunityExistsAsync(
        Guid communityId)
    {
        await using var scope =
            _factory.Services.CreateAsyncScope();

        var db = scope.ServiceProvider
            .GetRequiredService<SharedThingsDbContext>();

        return await db.Communities
            .AnyAsync(c => c.Id == communityId);
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
    
    [Fact]
    public async Task CreateInvitation_ReturnsInvitationForCommunityMember()
    {
        AuthenticateAs(SeedIds.Bill);

        var response = await _client.PostAsync(
            $"/api/communities/{SeedIds.Neighbourhood}/invitations",
            content: null);

        var invitation = await response.Content
            .ReadFromJsonAsync<CreateCommunityInvitationResponse>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(invitation);
        Assert.Equal(
            SeedIds.Neighbourhood,
            invitation.CommunityId);
        Assert.False(string.IsNullOrWhiteSpace(invitation.Token));
        Assert.True(invitation.ExpiresAt > DateTimeOffset.UtcNow);
        Assert.Equal(
            $"/api/invitations/{invitation.Token}",
            response.Headers.Location?.ToString());
    }
    
    [Fact]
    public async Task CreateInvitation_DoesNotRevealCommunityToNonMember()
    {
        AuthenticateAs(SeedIds.Casey);

        var response = await _client.PostAsync(
            $"/api/communities/{SeedIds.Neighbourhood}/invitations",
            content: null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task CreateInvitation_AllowsCreatorOfNewCommunity()
    {
        AuthenticateAs(SeedIds.Casey);

        var createCommunityResponse =
            await _client.PostAsJsonAsync(
                "/api/communities",
                new CreateCommunityRequest("Casey's Community"));

        var community = await createCommunityResponse.Content
            .ReadFromJsonAsync<CommunityResponse>();

        var invitationResponse = await _client.PostAsync(
            $"/api/communities/{community!.Id}/invitations",
            content: null);

        Assert.Equal(
            HttpStatusCode.Created,
            invitationResponse.StatusCode);
    }
    
    [Fact]
    public async Task GetInvitation_ReturnsCommunityPreview()
    {
        AuthenticateAs(SeedIds.Bill);

        var createResponse = await _client.PostAsync(
            $"/api/communities/{SeedIds.Neighbourhood}/invitations",
            content: null);

        var created = await createResponse.Content
            .ReadFromJsonAsync<CreateCommunityInvitationResponse>();

        AuthenticateAs(SeedIds.Casey);

        var response = await _client.GetAsync(
            $"/api/invitations/{created!.Token}");

        var preview = await response.Content
            .ReadFromJsonAsync<CommunityInvitationPreviewResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(preview);
        Assert.Equal(
            SeedIds.Neighbourhood,
            preview.CommunityId);
        Assert.Equal(
            "Our Neighbourhood",
            preview.CommunityName);
        Assert.False(preview.AlreadyMember);
    }
    
    [Fact]
    public async Task GetInvitation_IdentifiesExistingMember()
    {
        AuthenticateAs(SeedIds.Bill);

        var createResponse = await _client.PostAsync(
            $"/api/communities/{SeedIds.Neighbourhood}/invitations",
            content: null);

        var created = await createResponse.Content
            .ReadFromJsonAsync<CreateCommunityInvitationResponse>();

        var response = await _client.GetAsync(
            $"/api/invitations/{created!.Token}");

        var preview = await response.Content
            .ReadFromJsonAsync<CommunityInvitationPreviewResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(preview!.AlreadyMember);
    }
    
    [Fact]
    public async Task GetInvitation_ReturnsNotFoundForUnknownToken()
    {
        AuthenticateAs(SeedIds.Casey);

        var response = await _client.GetAsync(
            "/api/invitations/not-a-real-invitation");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task GetInvitation_DoesNotAddUserToCommunity()
    {
        AuthenticateAs(SeedIds.Bill);

        var createResponse = await _client.PostAsync(
            $"/api/communities/{SeedIds.Neighbourhood}/invitations",
            content: null);

        var created = await createResponse.Content
            .ReadFromJsonAsync<CreateCommunityInvitationResponse>();

        AuthenticateAs(SeedIds.Casey);

        await _client.GetAsync(
            $"/api/invitations/{created!.Token}");

        var communities = await _client
            .GetFromJsonAsync<CommunityResponse[]>(
                "/api/communities");

        Assert.DoesNotContain(
            communities!,
            community =>
                community.Id == SeedIds.Neighbourhood);
    }
    
    [Fact]
    public async Task AcceptInvitation_AddsUserToCommunity()
    {
        AuthenticateAs(SeedIds.Bill);

        var communityResponse = await _client.PostAsJsonAsync(
            "/api/communities",
            new CreateCommunityRequest("Repair Café"));

        var community = await communityResponse.Content
            .ReadFromJsonAsync<CommunityResponse>();

        var invitationResponse = await _client.PostAsync(
            $"/api/communities/{community!.Id}/invitations",
            content: null);

        var invitation = await invitationResponse.Content
            .ReadFromJsonAsync<CreateCommunityInvitationResponse>();

        AuthenticateAs(SeedIds.Casey);

        var acceptResponse = await _client.PostAsync(
            $"/api/invitations/{invitation!.Token}/accept",
            content: null);

        var accepted = await acceptResponse.Content
            .ReadFromJsonAsync<AcceptCommunityInvitationResponse>();

        Assert.Equal(HttpStatusCode.OK, acceptResponse.StatusCode);
        Assert.NotNull(accepted);
        Assert.Equal(community.Id, accepted.CommunityId);
        Assert.True(accepted.MembershipCreated);

        var communities = await _client
            .GetFromJsonAsync<CommunityResponse[]>(
                "/api/communities");

        Assert.Contains(
            communities!,
            result => result.Id == community.Id);
    }
    
    [Fact]
    public async Task AcceptInvitation_IsIdempotentForExistingMember()
    {
        AuthenticateAs(SeedIds.Bill);

        var invitationResponse = await _client.PostAsync(
            $"/api/communities/{SeedIds.Neighbourhood}/invitations",
            content: null);

        var invitation = await invitationResponse.Content
            .ReadFromJsonAsync<CreateCommunityInvitationResponse>();

        var firstResponse = await _client.PostAsync(
            $"/api/invitations/{invitation!.Token}/accept",
            content: null);

        var secondResponse = await _client.PostAsync(
            $"/api/invitations/{invitation.Token}/accept",
            content: null);

        var first = await firstResponse.Content
            .ReadFromJsonAsync<AcceptCommunityInvitationResponse>();

        var second = await secondResponse.Content
            .ReadFromJsonAsync<AcceptCommunityInvitationResponse>();

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);
        Assert.False(first!.MembershipCreated);
        Assert.False(second!.MembershipCreated);
    }
    
    [Fact]
    public async Task LeaveCommunity_RemovesCurrentUsersMembership()
    {
        AuthenticateAs(SeedIds.Bill);

        var response = await _client.DeleteAsync(
            $"/api/communities/{SeedIds.Neighbourhood}/membership");

        Assert.Equal(
            HttpStatusCode.NoContent,
            response.StatusCode);

        var communityResponse = await _client.GetAsync(
            $"/api/communities/{SeedIds.Neighbourhood}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            communityResponse.StatusCode);
    }
    
    [Fact]
    public async Task LeaveCommunity_ReturnsNotFoundWhenUserIsNotAMember()
    {
        AuthenticateAs(SeedIds.Casey);

        var response = await _client.DeleteAsync(
            $"/api/communities/{SeedIds.Neighbourhood}/membership");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);

        AuthenticateAs(SeedIds.Alex);

        var communityResponse = await _client.GetAsync(
            $"/api/communities/{SeedIds.Neighbourhood}");

        Assert.Equal(
            HttpStatusCode.OK,
            communityResponse.StatusCode);
    }
    
    [Fact]
    public async Task LeaveCommunity_ReturnsNotFoundWhenCommunityDoesNotExist()
    {
        AuthenticateAs(SeedIds.Bill);

        var response = await _client.DeleteAsync(
            $"/api/communities/{Guid.NewGuid()}/membership");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }
    
    [Fact]
    public async Task LeaveCommunity_RequiresAuthentication()
    {
        var response = await _client.DeleteAsync(
            $"/api/communities/{SeedIds.Neighbourhood}/membership");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }
    
    [Fact]
    public async Task LeaveCommunity_DoesNotDeleteUsersItems()
    {
        AuthenticateAs(SeedIds.Bill);

        var leaveResponse = await _client.DeleteAsync(
            $"/api/communities/{SeedIds.Neighbourhood}/membership");

        Assert.Equal(
            HttpStatusCode.NoContent,
            leaveResponse.StatusCode);

        var itemsResponse = await _client.GetAsync(
            "/api/items/myItems");

        Assert.Equal(
            HttpStatusCode.OK,
            itemsResponse.StatusCode);

        var items = await itemsResponse.Content
            .ReadFromJsonAsync<List<ItemSummaryResponse>>();

        Assert.NotNull(items);
        Assert.Contains(
            items,
            item => item.Id == SeedIds.CordlessDrill);
    }
    
    [Fact]
    public async Task LeaveCommunity_DeletesCommunityWhenUserIsFinalMember()
    {
        AuthenticateAs(SeedIds.Bill);

        var createResponse = await _client.PostAsJsonAsync(
            "/api/communities",
            new CreateCommunityRequest(
                "Temporary community"));

        createResponse.EnsureSuccessStatusCode();

        var community =
            await createResponse.Content
                .ReadFromJsonAsync<CommunityResponse>();

        Assert.NotNull(community);

        var leaveResponse = await _client.DeleteAsync(
            $"/api/communities/{community.Id}/membership");

        Assert.Equal(
            HttpStatusCode.NoContent,
            leaveResponse.StatusCode);

        var communityExists = await CommunityExistsAsync(
            community.Id);

        Assert.False(communityExists);
    }
    
}
