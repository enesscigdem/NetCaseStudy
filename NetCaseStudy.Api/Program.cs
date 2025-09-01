using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;
using System.Globalization;                              
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;                      
using Microsoft.IdentityModel.Tokens;
using NetCaseStudy.Api.Authorization;
using NetCaseStudy.Api.Infrastructure.Logging;
using NetCaseStudy.Api.Infrastructure.Middlewares;
using Serilog;
using NetCaseStudy.Api.Middlewares;
using NetCaseStudy.Api.Services;
using NetCaseStudy.Application.Abstractions;
using NetCaseStudy.Application.DTOs;
using NetCaseStudy.Application.Mapping;
using NetCaseStudy.Infrastructure.Cache;
using NetCaseStudy.Infrastructure.Identity;
using NetCaseStudy.Infrastructure.Persistence;
using NetCaseStudy.Api.Infrastructure.Swagger;           

var builder = WebApplication.CreateBuilder(args);


builder.Host.UseSerilog((ctx, lc) =>
{
    lc.ReadFrom.Configuration(ctx.Configuration)
        .Enrich.With<CorrelationIdEnricher>();
});

var redisConn = builder.Configuration.GetConnectionString("Redis");
string? connectionString = null;

if (builder.Environment.IsEnvironment("Test"))
{
    builder.Services.AddDbContext<ApplicationDbContext>(o =>
        o.UseInMemoryDatabase("NetCaseStudy_TestDb"));
    builder.Services.AddDbContext<AppIdentityDbContext>(o =>
        o.UseInMemoryDatabase("NetCaseStudy_IdentityTestDb"));

    builder.Services.AddHealthChecks()
        .AddCheck("self", () => HealthCheckResult.Healthy());
}
else
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;

    builder.Services.AddDbContext<ApplicationDbContext>(o =>
        o.UseSqlServer(connectionString, b => b.MigrationsAssembly("NetCaseStudy.Infrastructure")));
    builder.Services.AddDbContext<AppIdentityDbContext>(o =>
        o.UseSqlServer(connectionString));

    builder.Services.AddHealthChecks()
        .AddSqlServer(connectionString, name: "sqlserver")
        .AddRedis(redisConn!, name: "redis");

    builder.Services.AddStackExchangeRedisCache(options => { options.Configuration = redisConn; });
    if (!string.IsNullOrWhiteSpace(redisConn))
    {
        builder.Services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(
            _ => StackExchange.Redis.ConnectionMultiplexer.Connect(redisConn));
    }
    builder.Services.AddScoped<ICacheService, RedisCacheService>();
}

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireDigit = false;
    })
    .AddEntityFrameworkStores<AppIdentityDbContext>()
    .AddDefaultTokenProviders();

var jwtSection = builder.Configuration.GetSection("Jwt");
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = signingKey,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidAudience = jwtSection["Audience"],
        ValidateLifetime = true
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ViewOrder", policy => policy.Requirements.Add(new ViewOrderRequirement()));
});
builder.Services.AddScoped<IAuthorizationHandler, OrderAuthorizationHandler>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<IIdentityService, IdentityService>();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(ProductDto).Assembly));
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

builder.Services.AddValidatorsFromAssemblyContaining<NetCaseStudy.Application.Validators.CreateProductRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddLocalization(opt => opt.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var cultures = new[] { new CultureInfo("en-US"), new CultureInfo("tr-TR") };
    options.SupportedCultures = cultures;
    options.SupportedUICultures = cultures;
    options.DefaultRequestCulture = new("en-US");
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var user = httpContext.User;
        var isAdmin = user?.IsInRole("Admin") ?? false;
        if (isAdmin)
            return RateLimitPartition.GetNoLimiter("admin");

        string key = user?.Identity?.IsAuthenticated == true
            ? (user.FindFirst("sub")?.Value ?? user.Identity!.Name ?? "anon")
            : (httpContext.Connection.RemoteIpAddress?.ToString() ?? "anon");

        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 100,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        });
    });

    options.AddPolicy<string>("WritePolicy", context =>
    {
        var user = context.User;
        var isAdmin = user?.IsInRole("Admin") ?? false;
        if (isAdmin)
            return RateLimitPartition.GetNoLimiter("admin-write");

        string key = user?.Identity?.IsAuthenticated == true
            ? (user.FindFirst("sub")?.Value ?? user.Identity!.Name ?? "anon")
            : (context.Connection.RemoteIpAddress?.ToString() ?? "anon");

        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 20,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        });
    });
});


builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        { new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            { Reference = new Microsoft.OpenApi.Models.OpenApiReference
                { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } },
          Array.Empty<string>() }
    });

    options.OperationFilter<AcceptLanguageHeaderOperationFilter>();
});

var app = builder.Build();

await IdentitySeed.SeedRolesAndAdminAsync(app.Services);

app.UseSerilogRequestLogging();

app.UseMiddleware<ProblemDetailsMiddleware>();

var locOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(locOptions.Value);

app.UseRateLimiter();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    var apiVersionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        foreach (var desc in apiVersionProvider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json", desc.GroupName.ToUpperInvariant());
        }
    });
}

if (Environment.GetEnvironmentVariable("MIGRATE_ON_STARTUP")?
        .Equals("true", StringComparison.OrdinalIgnoreCase) == true)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program { }
