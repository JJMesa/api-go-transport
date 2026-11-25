using GoTransport.Api.Component.Tests.Infrastructure;
using GoTransport.Domain.Entities.App;
using GoTransport.Domain.Entities.Bas;
using GoTransport.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GoTransport.Api.Component.Tests.TestCases;

/// <summary>
/// Checks the EF model the migrations are generated from. EF InMemory does not enforce unique indexes,
/// so the uniqueness rules are verified on the model rather than by inserting duplicates.
/// </summary>
public class DatabaseModelTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public DatabaseModelTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData(typeof(Department), new[] { nameof(Department.Description) })]
    [InlineData(typeof(City), new[] { nameof(City.DepartmentId), nameof(City.Description) })]
    [InlineData(typeof(Route), new[] { nameof(Route.OriginPointId), nameof(Route.DestinationPointId) })]
    [InlineData(typeof(Vehicle), new[] { nameof(Vehicle.LicensePlate) })]
    public void Model_HasAUniqueIndexBehindEachDuplicateCheck(Type entity, string[] properties)
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var model = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Model;

        // Act
        var indexes = model.FindEntityType(entity)!.GetIndexes();

        // Assert
        Assert.Contains(indexes, index => index.IsUnique && index.Properties.Select(p => p.Name).SequenceEqual(properties));
    }
}
