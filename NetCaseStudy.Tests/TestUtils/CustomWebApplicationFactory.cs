using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore; // <-- BUNU EKLE
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetCaseStudy.Api;
using NetCaseStudy.Application.Abstractions;
using NetCaseStudy.Infrastructure.Persistence;

namespace NetCaseStudy.Tests.TestUtils;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureAppConfiguration((ctx, cfg) =>
        {
            cfg.AddJsonFile("appsettings.Test.json", optional: true);
        });

        builder.ConfigureServices(services =>
        {
            var dbDescriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (dbDescriptor is not null) services.Remove(dbDescriptor);

            services.AddDbContext<ApplicationDbContext>(o =>
                o.UseInMemoryDatabase("NetCaseStudy_TestDb"));

            services.RemoveAll<ICacheService>();
            services.AddSingleton<ICacheService, FakeCacheService>();

            services.PostConfigure<HealthCheckServiceOptions>(opt =>
            {
                opt.Registrations.Clear();
                opt.Registrations.Add(new HealthCheckRegistration(
                    "self", sp => new SelfHealthCheck(), null, null));
            });

            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
            TestDataSeeder.SeedBasicProducts(db);
        });
    }
}