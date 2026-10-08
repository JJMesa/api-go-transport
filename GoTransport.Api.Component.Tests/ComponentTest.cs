using System.Net.Http.Headers;
using System.Net.Mime;
using GoTransport.Api.Component.Tests.Infrastructure;
using Newtonsoft.Json;

namespace GoTransport.Api.Component.Tests;

public abstract class ComponentTest : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly HttpClient Client;

    protected JsonSerializerSettings JsonSettings = new()
    {
        NullValueHandling = NullValueHandling.Ignore
    };

    protected ComponentTest(CustomWebApplicationFactory factory)
    {
        factory.ResetDatabase();

        Client = factory.CreateClient();
        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
    }

    /// <summary>
    /// Marks subsequent requests as authenticated for the test authentication scheme. Kept as an
    /// awaitable member so the existing call sites that predate the in-memory setup remain valid.
    /// </summary>
    protected Task AddAuthorization()
    {
        if (!Client.DefaultRequestHeaders.Contains(TestAuthHandler.AuthHeader))
            Client.DefaultRequestHeaders.Add(TestAuthHandler.AuthHeader, "true");

        return Task.CompletedTask;
    }
}
