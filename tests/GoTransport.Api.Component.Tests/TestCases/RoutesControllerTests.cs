using System.Net;
using System.Net.Mime;
using System.Text;
using GoTransport.Api.Component.Tests.Infrastructure;
using GoTransport.Api.Test.Utilities.Commons;
using GoTransport.Application.Dtos.Route;
using GoTransport.Application.Wrappers;
using Newtonsoft.Json;

namespace GoTransport.Api.Component.Tests.TestCases;

public class RoutesControllerTests : ComponentTest
{
    public RoutesControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    #region GetAsync

    [Theory]
    [InlineData("destinationPointSearchCriteria=Bogot", 1)]
    [InlineData("destinationPointSearchCriteria=Salitre", 1)]
    [InlineData("destinationPointSearchCriteria=Cali", 0)]
    [InlineData("originPointSearchCriteria=Medell", 1)]
    [InlineData("originPointSearchCriteria=Medell&destinationPointSearchCriteria=Cali", 0)]
    [InlineData("originPointSearchCriteria=Bogot&destinationPointSearchCriteria=Medell", 0)]
    public async Task GetAsync_SearchesOriginAndDestinationSeparately(string query, int expected)
    {
        // Act
        await AddAuthorization();
        var apiResponse = await Client.GetAsync($"{ApiPaths.RoutesFullPath}?{query}");
        var jsonResponse = await apiResponse.Content.ReadAsStringAsync();
        var actual = JsonConvert.DeserializeObject<JsonPagedResponse<List<RouteDto>>>(jsonResponse, JsonSettings);

        // Assert
        Assert.Equal(HttpStatusCode.OK, apiResponse.StatusCode);
        Assert.Equal(expected, actual?.Data?.Count);
    }

    #endregion GetAsync

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_WithTheSameOriginAndDestination_BadRequest()
    {
        // Arrange
        var route = new RouteCreationDto { Description = "Norte - Norte", OriginPointId = 1, DestinationPointId = 1 };

        // Act
        await AddAuthorization();
        var apiResponse = await Client.PostAsync(ApiPaths.RoutesFullPath, JsonContent(route));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, apiResponse.StatusCode);
    }

    [Fact]
    public async Task CreateAsync_WithDifferentPoints_Created()
    {
        // Arrange
        var route = new RouteCreationDto { Description = "Bogota - Medellin", OriginPointId = 2, DestinationPointId = 1 };

        // Act
        await AddAuthorization();
        var apiResponse = await Client.PostAsync(ApiPaths.RoutesFullPath, JsonContent(route));

        // Assert
        Assert.Equal(HttpStatusCode.Created, apiResponse.StatusCode);
    }

    #endregion CreateAsync

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_WithTheSameOriginAndDestination_BadRequest()
    {
        // Arrange
        var route = new RouteUpdateDto { RouteId = 1, Description = "Salitre - Salitre", OriginPointId = 2, DestinationPointId = 2, IsActive = true };

        // Act
        await AddAuthorization();
        var apiResponse = await Client.PutAsync($"{ApiPaths.RoutesFullPath}/1", JsonContent(route));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, apiResponse.StatusCode);
    }

    [Fact]
    public async Task UpdateAsync_Ok()
    {
        // Arrange: same Points, new description (its own Route is not a duplicate of itself)
        var route = new RouteUpdateDto { RouteId = 1, Description = "Medellín - Bogotá (directo)", OriginPointId = 1, DestinationPointId = 2, IsActive = true };

        // Act
        await AddAuthorization();
        var apiResponse = await Client.PutAsync($"{ApiPaths.RoutesFullPath}/1", JsonContent(route));
        var actual = JsonConvert.DeserializeObject<JsonResponse<RouteDto>>(await apiResponse.Content.ReadAsStringAsync(), JsonSettings);

        // Assert
        Assert.Equal(HttpStatusCode.OK, apiResponse.StatusCode);
        Assert.Equal("Medellín - Bogotá (directo)", actual!.Data!.Description);
    }

    #endregion UpdateAsync

    private static StringContent JsonContent(object body) =>
        new(JsonConvert.SerializeObject(body), Encoding.UTF8, MediaTypeNames.Application.Json);
}
