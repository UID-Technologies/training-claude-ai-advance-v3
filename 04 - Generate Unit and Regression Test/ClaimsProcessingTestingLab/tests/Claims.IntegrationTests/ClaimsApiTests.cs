using System.Net;
using System.Net.Http.Json;
using Claims.Application;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Claims.IntegrationTests;

public sealed class ClaimsApiTests :
    IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ClaimsApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // Intentionally incomplete.
    // Students will generate POST/GET/error-path integration tests with Claude.
}
