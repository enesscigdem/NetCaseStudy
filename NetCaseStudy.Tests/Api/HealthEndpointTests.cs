using System.Net;
using NetCaseStudy.Tests.TestUtils;

namespace NetCaseStudy.Tests.Api;

public class HealthEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    public HealthEndpointTests(CustomWebApplicationFactory factory)
        => _client = factory.CreateClient();

    [Fact]
    public async Task Health_Should_Return_Healthy_In_Test()
    {
        var res = await _client.GetAsync("/health");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}