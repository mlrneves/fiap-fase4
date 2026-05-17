using System.Net;
using FluentAssertions;

namespace Tests;

public class SmokeTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SmokeTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_Swagger_Should_Return_Valid_Response()
    {
        var response = await _client.GetAsync("/swagger");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Moved,
            HttpStatusCode.Redirect,
            HttpStatusCode.TemporaryRedirect,
            HttpStatusCode.PermanentRedirect,
            HttpStatusCode.NotFound);
    }
}