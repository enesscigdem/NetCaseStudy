using System.Net;
using NetCaseStudy.Application.DTOs;
using NetCaseStudy.Tests.TestUtils;

namespace NetCaseStudy.Tests.Api;

public class ProductsApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    public ProductsApiTests(CustomWebApplicationFactory factory)
        => _client = factory.CreateClient();

    [Fact]
    public async Task Get_Products_Should_Support_ETag_304()
    {
        var res1 = await _client.GetAsync("/api/v1/Products?page=1&pageSize=2");
        res1.StatusCode.Should().Be(HttpStatusCode.OK);

        res1.Headers.ETag.Should().NotBeNull();
        var etag = res1.Headers.ETag!.Tag;

        var req = new HttpRequestMessage(HttpMethod.Get, "/api/v1/Products?page=1&pageSize=2");
        req.Headers.TryAddWithoutValidation("If-None-Match", etag);
        var res2 = await _client.SendAsync(req);

        res2.StatusCode.Should().Be(HttpStatusCode.NotModified);
    }

    [Fact]
    public async Task Get_Products_With_Cursor_Should_Paginate()
    {
        var first = await _client.GetFromJsonAsync<CursorPagedResult<ProductDto>>(
            "/api/v1/Products/cursor?pageSize=2");

        first.Should().NotBeNull();
        first!.Items.Count.Should().BeGreaterThan(0);
        first.PageSize.Should().Be(2);

        if (first.NextCursor is not null)
        {
            var next = await _client.GetFromJsonAsync<CursorPagedResult<ProductDto>>(
                $"/api/v1/Products/cursor?pageSize=2&cursor={Uri.EscapeDataString(first.NextCursor)}");

            next.Should().NotBeNull();
            next!.Items.Should().NotBeEquivalentTo(first.Items);
        }
    }
}