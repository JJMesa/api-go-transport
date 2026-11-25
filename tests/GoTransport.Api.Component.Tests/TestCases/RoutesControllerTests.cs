using System.Net;
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
}
