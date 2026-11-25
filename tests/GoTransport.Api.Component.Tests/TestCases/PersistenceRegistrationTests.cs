using GoTransport.Domain.Interfaces;
using GoTransport.Persistence;
using GoTransport.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GoTransport.Api.Component.Tests.TestCases;

/// <summary>
/// Checks the production SQL Server registration, which the other component tests replace with InMemory.
/// Nothing here opens a connection.
/// </summary>
public class PersistenceRegistrationTests
{
    [Fact]
    public void AddPersistenceLayer_RetriesTransientSqlServerFailures()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=unused;Database=unused;Integrated Security=true"
            })
            .Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<ICurrentUser>(new SystemUser());
        services.AddPersistenceLayer(configuration);

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Act
        var strategy = context.Database.CreateExecutionStrategy();

        // Assert
        Assert.IsType<SqlServerRetryingExecutionStrategy>(strategy);
    }

    private sealed class SystemUser : ICurrentUser
    {
        public string? UserId => null;
    }
}
