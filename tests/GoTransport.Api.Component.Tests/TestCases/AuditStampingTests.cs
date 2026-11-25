using GoTransport.Api.Component.Tests.Infrastructure;
using GoTransport.Application.Commons;
using GoTransport.Domain.Entities.Bas;
using GoTransport.Persistence.Contexts;
using Microsoft.Extensions.DependencyInjection;

namespace GoTransport.Api.Component.Tests.TestCases;

public class AuditStampingTests : ComponentTest
{
    private readonly CustomWebApplicationFactory _factory;

    public AuditStampingTests(CustomWebApplicationFactory factory) : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public void SaveChanges_StampsTheAuditColumnsLikeSaveChangesAsync()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var department = new Department { DepartmentId = 90, Description = "Sync save" };
        context.Add(department);

        // Act: the synchronous overload, which nothing used to stamp
        context.SaveChanges();

        // Assert
        var saved = FindInDatabase<Department>(90)!;
        Assert.True(saved.IsActive);
        Assert.Equal(Constants.ApplicationUser, saved.CreationUser);
        Assert.Equal(Constants.ApplicationUser, saved.UpdateUser);
        Assert.NotEqual(default, saved.CreatedAt);
    }
}
