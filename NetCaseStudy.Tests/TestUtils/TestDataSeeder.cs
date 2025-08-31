using NetCaseStudy.Infrastructure.Persistence;
using NetCaseStudy.Domain.Entities;

namespace NetCaseStudy.Tests.TestUtils;

public static class TestDataSeeder
{
    public static void SeedBasicProducts(ApplicationDbContext db)
    {
        if (db.Products.Any()) return;

        db.Products.AddRange(
            new Product { Id=1, Name="Kalem",  Price=10, IsActive=true,  IsDeleted=false },
            new Product { Id=2, Name="Defter", Price=20, IsActive=true,  IsDeleted=false },
            new Product { Id=3, Name="Silgi",  Price=5,  IsActive=true,  IsDeleted=false },
            new Product { Id=4, Name="Cetvel", Price=15, IsActive=true,  IsDeleted=false }
        );
        db.SaveChanges();
    }
}