using System.Net;
using System.Net.Http.Json;
using SharedThings.Api.Contracts;
using Xunit;

namespace SharedThings.Api.Tests;


public sealed class SiteTests :
    IClassFixture<SharedThingsApiFactory>
{
    private readonly SharedThingsApiFactory _factory;
    private readonly HttpClient _client;

    public SiteTests(
        SharedThingsApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task Health_ReturnsOkWithoutAuthentication()
    {
        _client.DefaultRequestHeaders.Remove("X-User-Id");

        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task Register_AuthenticatesNewUser()
    {
        var registerResponse = await _client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest(
                "new-user@example.com",
                "Password1",
                "New User"));

        var meResponse =
            await _client.GetAsync("/api/auth/me");

        Assert.Equal(
            HttpStatusCode.Created,
            registerResponse.StatusCode);

        Assert.Equal(
            HttpStatusCode.OK,
            meResponse.StatusCode);
    }

}