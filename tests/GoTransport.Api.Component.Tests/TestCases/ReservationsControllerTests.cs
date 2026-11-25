using System.Net;
using System.Net.Mime;
using System.Text;
using GoTransport.Api.Component.Tests.Infrastructure;
using GoTransport.Api.Test.Utilities.Commons;
using GoTransport.Application.Commons;
using GoTransport.Application.Dtos.Reservation;
using GoTransport.Application.Wrappers;
using Newtonsoft.Json;

namespace GoTransport.Api.Component.Tests.TestCases;

public class ReservationsControllerTests : ComponentTest
{
    public ReservationsControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    #region GetByScheduleAsync

    [Fact]
    public async Task GetByScheduleAsync_Ok()
    {
        // Act
        await AddAuthorization();
        var apiResponse = await Client.GetAsync($"{ApiPaths.SchedulesFullPath}/{DatabaseSeeder.ScheduleId}{ApiPaths.ReservationsPath}");
        var jsonResponse = await apiResponse.Content.ReadAsStringAsync();
        var actual = JsonConvert.DeserializeObject<JsonPagedResponse<List<ReservationDto>>>(jsonResponse, JsonSettings);

        // Assert
        Assert.Equal(HttpStatusCode.OK, apiResponse.StatusCode);
        Assert.Equal(2, actual?.Data?.Count);
    }

    [Fact]
    public async Task GetByScheduleAsync_ReturnsTheTravelDates()
    {
        // Act
        await AddAuthorization();
        var apiResponse = await Client.GetAsync($"{ApiPaths.SchedulesFullPath}/{DatabaseSeeder.ScheduleId}{ApiPaths.ReservationsPath}");
        var jsonResponse = await apiResponse.Content.ReadAsStringAsync();
        var actual = JsonConvert.DeserializeObject<JsonPagedResponse<List<ReservationDto>>>(jsonResponse, JsonSettings);

        // Assert
        Assert.Equal(HttpStatusCode.OK, apiResponse.StatusCode);
        Assert.Equal(
            new[] { DateTime.Today.AddDays(-7), DateTime.Today.AddDays(7) },
            actual!.Data!.Select(reservation => reservation.ReservationDate).OrderBy(date => date));
    }

    [Fact]
    public async Task GetByScheduleAsync_WithoutAuthentication_Unauthorized()
    {
        // Act
        var apiResponse = await Client.GetAsync($"{ApiPaths.SchedulesFullPath}/{DatabaseSeeder.ScheduleId}{ApiPaths.ReservationsPath}");
        var jsonResponse = await apiResponse.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, apiResponse.StatusCode);
        Assert.DoesNotContain("1000123456", jsonResponse);
    }

    #endregion GetByScheduleAsync

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ReturnsTheTravelDate()
    {
        // Act
        await AddAuthorization();
        var apiResponse = await Client.GetAsync($"{ApiPaths.ReservationsFullPath}/{DatabaseSeeder.UpcomingReservationId}");
        var jsonResponse = await apiResponse.Content.ReadAsStringAsync();
        var actual = JsonConvert.DeserializeObject<JsonResponse<ReservationDto>>(jsonResponse, JsonSettings);

        // Assert
        Assert.Equal(HttpStatusCode.OK, apiResponse.StatusCode);
        Assert.Equal(DateTime.Today.AddDays(7), actual?.Data?.ReservationDate);
    }

    #endregion GetByIdAsync

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_OnAnotherDateOfTheSchedule_Ok()
    {
        // Arrange: the seeded Schedule already has as many Reservations as the Vehicle has places, on two other dates.
        var travelDate = DateTime.Today.AddDays(14);

        // Act
        await AddAuthorization();
        var apiResponse = await Client.PostAsync(ApiPaths.ReservationsFullPath, JsonContent(NewReservation(travelDate)));
        var jsonResponse = await apiResponse.Content.ReadAsStringAsync();
        var actual = JsonConvert.DeserializeObject<JsonResponse<ReservationDto>>(jsonResponse, JsonSettings);

        // Assert
        Assert.Equal(HttpStatusCode.OK, apiResponse.StatusCode);
        Assert.Equal(travelDate, actual?.Data?.ReservationDate);
    }

    [Fact]
    public async Task CreateAsync_OnAFullTrip_Conflict()
    {
        // Arrange: the upcoming Trip has one seeded Reservation; this booking takes its last place.
        await AddAuthorization();
        var lastPlace = await Client.PostAsync(ApiPaths.ReservationsFullPath, JsonContent(NewReservation(DatabaseSeeder.UpcomingTravelDate)));

        // Act
        var apiResponse = await Client.PostAsync(ApiPaths.ReservationsFullPath, JsonContent(NewReservation(DatabaseSeeder.UpcomingTravelDate)));
        var jsonResponse = await apiResponse.Content.ReadAsStringAsync();
        var actual = JsonConvert.DeserializeObject<JsonResponse<ReservationDto>>(jsonResponse, JsonSettings);

        // Assert
        Assert.Equal(HttpStatusCode.OK, lastPlace.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, apiResponse.StatusCode);
        Assert.Contains(ErrorMessages.VehicleFull, actual?.Errors!);
    }

    [Fact]
    public async Task CreateAsync_WithUnknownSchedule_NotFound()
    {
        // Arrange
        var reservation = NewReservation(DateTime.Today.AddDays(14));
        reservation.ScheduleId = Guid.NewGuid();

        // Act
        await AddAuthorization();
        var apiResponse = await Client.PostAsync(ApiPaths.ReservationsFullPath, JsonContent(reservation));
        var jsonResponse = await apiResponse.Content.ReadAsStringAsync();
        var actual = JsonConvert.DeserializeObject<JsonResponse<ReservationDto>>(jsonResponse, JsonSettings);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, apiResponse.StatusCode);
        Assert.Contains(ErrorMessages.InvalidSchedule, actual?.Errors!);
    }

    #endregion CreateAsync

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

    #region Helpers

    private static ReservationCreationDto NewReservation(DateTime travelDate) => new()
    {
        ScheduleId = DatabaseSeeder.ScheduleId,
        ReservationDate = travelDate,
        PassengerFirstName = "Jane",
        PassengerLastName = "Roe",
        IdentificationTypeId = 1,
        PassengerIdentification = "1000654321",
        Detail = "Window seat"
    };

    private static StringContent JsonContent(object body) =>
        new(JsonConvert.SerializeObject(body), Encoding.UTF8, MediaTypeNames.Application.Json);

    #endregion Helpers
}
