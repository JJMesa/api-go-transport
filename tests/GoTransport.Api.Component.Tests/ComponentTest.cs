using System.Net.Http.Headers;
using System.Net.Mime;
using GoTransport.Api.Component.Tests.Infrastructure;
using GoTransport.Application.Commons;
using GoTransport.Persistence.Contexts;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace GoTransport.Api.Component.Tests;

public abstract class ComponentTest : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly HttpClient Client;
    private readonly CustomWebApplicationFactory _factory;

    protected JsonSerializerSettings JsonSettings = new()
    {
        NullValueHandling = NullValueHandling.Ignore
    };

    protected ComponentTest(CustomWebApplicationFactory factory)
    {
        factory.ResetDatabase();

        _factory = factory;
        Client = factory.CreateClient();
        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
    }

    /// <summary>
    /// Marks subsequent requests as authenticated for the test authentication scheme, as a user with
    /// <paramref name="role"/> (an <c>Administrador</c> by default; <see cref="TestAuthHandler.NoRole"/>
    /// for none). Kept awaitable so the call sites that predate the in-memory setup remain valid.
    /// </summary>
    protected Task AddAuthorization(string role = Roles.Administrator)
    {
        Client.DefaultRequestHeaders.Remove(TestAuthHandler.AuthHeader);
        Client.DefaultRequestHeaders.Add(TestAuthHandler.AuthHeader, role);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Reads an entity straight from the test database, bypassing the API. Use it to assert on
    /// state the DTOs do not expose, such as the audit columns.
    /// </summary>
    protected TEntity? FindInDatabase<TEntity>(params object[] keyValues) where TEntity : class
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return context.Set<TEntity>().Find(keyValues);
    }
}
