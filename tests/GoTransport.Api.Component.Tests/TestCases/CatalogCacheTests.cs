using System.Net;
using System.Net.Mime;
using System.Text;
using GoTransport.Api.Component.Tests.Infrastructure;
using GoTransport.Api.Test.Utilities.Commons;
using GoTransport.Application.Dtos.Point;
using GoTransport.Application.Dtos.Route;
using GoTransport.Application.Wrappers;
using Newtonsoft.Json;

namespace GoTransport.Api.Component.Tests.TestCases;

/// <summary>
/// The "/all" catalogs are cached in memory; a write must drop every cached list it makes stale.
/// </summary>
public class CatalogCacheTests : ComponentTest
{
    public CatalogCacheTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreatingAPoint_ShowsItInTheCachedPointList()
    {
        // Arrange: fill the cache
        await AddAuthorization();
        var before = await GetAllAsync<PointDto>(ApiPaths.PointsFullPath);

        // Act
        var created = await Client.PostAsync(ApiPaths.PointsFullPath, JsonContent(new PointCreationDto { CityId = 3, Detail = "Terminal Bello" }));
        var after = await GetAllAsync<PointDto>(ApiPaths.PointsFullPath);

        // Assert
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.Equal(before.Count + 1, after.Count);
        Assert.Contains(after, point => point.Detail == "Terminal Bello");
    }

    [Fact]
    public async Task UpdatingAPoint_RefreshesTheCachedRouteList()
    {
        // Arrange: fill the Route cache, whose Routes embed their Points
        await AddAuthorization();
        await GetAllAsync<RouteDto>(ApiPaths.RoutesFullPath);

        // Act
        var updated = await Client.PutAsync($"{ApiPaths.PointsFullPath}/1",
            JsonContent(new PointUpdateDto { PointId = 1, CityId = 1, Detail = "Terminal del Norte", IsActive = true }));
        var routes = await GetAllAsync<RouteDto>(ApiPaths.RoutesFullPath);

        // Assert
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        Assert.Equal("Terminal del Norte", routes.Single(route => route.RouteId == 1).OriginPoint!.Detail);
    }

    private async Task<List<T>> GetAllAsync<T>(string path)
    {
        var apiResponse = await Client.GetAsync($"{path}/all");
        Assert.Equal(HttpStatusCode.OK, apiResponse.StatusCode);
        var actual = JsonConvert.DeserializeObject<JsonResponse<List<T>>>(await apiResponse.Content.ReadAsStringAsync(), JsonSettings);
        return actual!.Data!;
    }

    private static StringContent JsonContent(object body) =>
        new(JsonConvert.SerializeObject(body), Encoding.UTF8, MediaTypeNames.Application.Json);
}
