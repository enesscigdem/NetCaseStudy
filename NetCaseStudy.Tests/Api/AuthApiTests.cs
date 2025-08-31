using System.Net;
using NetCaseStudy.Tests.TestUtils;

namespace NetCaseStudy.Tests.Api;

public class AuthApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    public AuthApiTests(CustomWebApplicationFactory factory)
        => _client = factory.CreateClient();

    [Fact]
    public async Task Register_Then_Login_Should_Return_Token()
    {
        var email = $"user{Guid.NewGuid():N}@test.com";
        var password = "P@ssw0rd!";

        var reg = await _client.PostAsJsonAsync("/api/v1/Auth/register", new {
            Email = email, Password = password, Role = "User"
        });
        reg.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var login = await _client.PostAsJsonAsync("/api/v1/Auth/login", new {
            Email = email, Password = password
        });
        login.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await login.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        body.Should().NotBeNull();
        body!["token"].Should().NotBeNullOrWhiteSpace();
    }
}