using GoTransport.Persistence.Contexts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GoTransport.Api.Component.Tests.Infrastructure;

/// <summary>
/// Boots the API for component tests against an isolated EF Core in-memory database and a test
/// authentication scheme, so the suite is fully reproducible and requires neither a real SQL Server
/// instance nor seeded credentials.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"GoTransportComponentTests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("ComponentTesting");

        builder.ConfigureTestServices(services =>
        {
            ReplaceDatabaseWithInMemory(services);
            ReplaceAuthenticationWithTestScheme(services);
        });
    }

    /// <summary>
    /// Restores the in-memory database to the known seed data. Called once before each test so
    /// mutations from one test never leak into another.
    /// </summary>
    public void ResetDatabase()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        DatabaseSeeder.Seed(context);
    }

    private void ReplaceDatabaseWithInMemory(IServiceCollection services)
    {
        var descriptors = services
            .Where(descriptor =>
                descriptor.ServiceType == typeof(DbContextOptions<ApplicationDbContext>)
                || descriptor.ServiceType == typeof(DbContextOptions))
            .ToList();

        foreach (var descriptor in descriptors)
            services.Remove(descriptor);

        services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(_databaseName));
    }

    private static void ReplaceAuthenticationWithTestScheme(IServiceCollection services)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
            options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
            options.DefaultScheme = TestAuthHandler.SchemeName;
        })
        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
    }
}