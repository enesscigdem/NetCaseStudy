using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetCaseStudy.Infrastructure.Persistence;
using NetCaseStudy.Domain.Entities;
using NetCaseStudy.Tests.TestUtils;

namespace NetCaseStudy.Tests.Api;

public class OrdersApiAuthzTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public OrdersApiAuthzTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> CreateUserAndGetTokenAsync(string email)
    {
        var password = "P@ssw0rd!";
        var reg = await _client.PostAsJsonAsync("/api/v1/Auth/register",
            new { Email = email, Password = password, Role = "User" });
        reg.EnsureSuccessStatusCode();

        var login = await _client.PostAsJsonAsync("/api/v1/Auth/login", new { Email = email, Password = password });
        login.EnsureSuccessStatusCode();
        var body = await login.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        return body!["token"];
    }

    [Fact]
    public async Task NonOwner_Should_Get_403_When_Accessing_Others_Order()
    {
        var tokenAlice = await CreateUserAndGetTokenAsync($"alice{Guid.NewGuid():N}@test.com");
        var tokenBob = await CreateUserAndGetTokenAsync($"bob{Guid.NewGuid():N}@test.com");

        string aliceId;
        int orderId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            aliceId = await db.Users.Where(u => u.Email!.StartsWith("alice"))
                .Select(u => u.Id).FirstAsync();

            db.Orders.Add(new Order { UserId = aliceId });
            await db.SaveChangesAsync();

            orderId = await db.Orders.OrderByDescending(o => o.Id)
                .Select(o => o.Id).FirstAsync();
        }

        var reqBob = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/Orders/{orderId}");
        reqBob.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenBob);
        var resBob = await _client.SendAsync(reqBob);
        resBob.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var reqAlice = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/Orders/{orderId}");
        reqAlice.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenAlice);
        var resAlice = await _client.SendAsync(reqAlice);
        resAlice.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}