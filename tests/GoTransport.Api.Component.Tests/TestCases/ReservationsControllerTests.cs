using System.Net;
using GoTransport.Api.Component.Tests.Infrastructure;
using GoTransport.Api.Test.Utilities.Commons;
using GoTransport.Application.Commons;
using GoTransport.Application.Wrappers;
using Newtonsoft.Json;

namespace GoTransport.Api.Component.Tests.TestCases;

public class ReservationsControllerTests : ComponentTest
{
    public ReservationsControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_UpcomingTrip_NoContent()
    {
        // Act
        await AddAuthorization();
        var apiResponse = await Client.DeleteAsync($"{ApiPaths.ReservationsFullPath}/{DatabaseSeeder.UpcomingReservationId}");
        var apiResponseGet = await Client.GetAsync($"{ApiPaths.ReservationsFullPath}/{DatabaseSeeder.UpcomingReservationId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, apiResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, apiResponseGet.StatusCode);
    }

    [Fact]
    public async Task DeleteAsync_WithPastTravelDate_BadRequest()
    {
        // Act
        await AddAuthorization();
        var apiResponse = await Client.DeleteAsync($"{ApiPaths.ReservationsFullPath}/{DatabaseSeeder.PastReservationId}");
        var jsonResponse = await apiResponse.Content.ReadAsStringAsync();
        var actual = JsonConvert.DeserializeObject<JsonResponse<bool?>>(jsonResponse, JsonSettings);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, apiResponse.StatusCode);
        Assert.Contains(ErrorMessages.ReservationDatePassed, actual?.Errors!);
    }

    [Fact]
    public async Task DeleteAsync_WithNonexistentId_NotFound()
    {
        // Act
        await AddAuthorization();
        var apiResponse = await Client.DeleteAsync($"{ApiPaths.ReservationsFullPath}/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, apiResponse.StatusCode);
    }

    #endregion DeleteAsync
}
