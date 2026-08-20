using System.Net;
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

}