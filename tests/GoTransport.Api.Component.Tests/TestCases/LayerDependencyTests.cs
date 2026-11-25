using GoTransport.Api.Middlewares;
using GoTransport.Application.Commons;
using GoTransport.Domain.Entities.App;
using GoTransport.Persistence.Contexts;

namespace GoTransport.Api.Component.Tests.TestCases;

/// <summary>
/// The layer rules, checked on the compiled assemblies: Domain is the bottom and knows no EF Core,
/// Persistence knows only Domain, and nothing below Api knows Api.
/// </summary>
public class LayerDependencyTests
{
    [Fact]
    public void Domain_ReferencesNoOtherLayerAndNoEfCore()
    {
        var references = ReferencedAssemblies(typeof(User));

        Assert.DoesNotContain(references, name => name.StartsWith("GoTransport."));
        Assert.DoesNotContain(references, name => name.StartsWith("Microsoft.EntityFrameworkCore"));
    }

    [Fact]
    public void Persistence_ReferencesOnlyTheDomainLayer()
    {
        var references = ReferencedAssemblies(typeof(ApplicationDbContext));

        Assert.Contains("GoTransport.Domain", references);
        Assert.DoesNotContain("GoTransport.Application", references);
        Assert.DoesNotContain("GoTransport.Api", references);
    }

    [Fact]
    public void Application_DoesNotReferencePersistenceOrApi()
    {
        var references = ReferencedAssemblies(typeof(ColombiaTime));

        Assert.DoesNotContain("GoTransport.Persistence", references);
        Assert.DoesNotContain("GoTransport.Api", references);
    }

    [Fact]
    public void Api_ComposesApplicationAndPersistence()
    {
        var references = ReferencedAssemblies(typeof(ErrorHandlerMiddleware));

        Assert.Contains("GoTransport.Application", references);
        Assert.Contains("GoTransport.Persistence", references);
    }

    private static List<string> ReferencedAssemblies(Type typeInLayer) =>
        typeInLayer.Assembly.GetReferencedAssemblies().Select(assembly => assembly.Name!).ToList();
}
