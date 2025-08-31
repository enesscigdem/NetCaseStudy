using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetCaseStudy.Application.Features.Products.Queries;
using NetCaseStudy.Application.Mapping;
using NetCaseStudy.Infrastructure.Persistence;
using NetCaseStudy.Tests.TestUtils;

namespace NetCaseStudy.Tests.Application;

public class ListProductsQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Filter_And_Sort()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("Products_UnitTestDb")
            .Options;

        await using var db = new ApplicationDbContext(options, new FakeCurrentUserService("test-user"));

        db.Products.AddRange(
            new NetCaseStudy.Domain.Entities.Product
                { Id = 1, Name = "Kalem", Price = 10, IsActive = true, IsDeleted = false },
            new NetCaseStudy.Domain.Entities.Product
                { Id = 2, Name = "Defter", Price = 20, IsActive = true, IsDeleted = false },
            new NetCaseStudy.Domain.Entities.Product
                { Id = 3, Name = "Silgi", Price = 5, IsActive = true, IsDeleted = false }
        );
        await db.SaveChangesAsync();

        var config = new MapperConfiguration(cfg => { cfg.AddProfile(new MappingProfile()); });
        var mapper = config.CreateMapper();


        var handler = new ListProductsQueryHandler(db, mapper);

        var query = new ListProductsQuery(1, 10, null, null, null, "price", false);
        var result = await handler.Handle(query, CancellationToken.None);

        result.TotalCount.Should().BeGreaterThan(0);
        result.Items.First().Name.Should().Be("Silgi");
    }
}