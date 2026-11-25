using System.Net;
using System.Net.Mime;
using System.Text;
using GoTransport.Api.Component.Tests.Infrastructure;
using GoTransport.Application.Dtos.Reservation;
using GoTransport.Application.Dtos.Route;
using GoTransport.Application.Dtos.Schedule;
using GoTransport.Application.Dtos.Vehicle;
using GoTransport.Application.Wrappers;
using Newtonsoft.Json;

namespace GoTransport.Api.Component.Tests.TestCases;

/// <summary>
/// Get-by-id and create responses carry the same navigations as the list endpoints. Unit-test mocks
/// return fully built entities, so only the real pipeline shows a missing Include.
/// </summary>
public class DetailResponseShapeTests : ComponentTest
{
    public DetailResponseShapeTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetRouteById_IncludesPointsAndCities()
    {
        var actual = await GetAsync<RouteDto>($"{ApiPaths.RoutesFullPath}/1");

        Assert.Equal("Medellín", actual.OriginPoint!.City!.Description);
        Assert.Equal("Terminal Salitre", actual.DestinationPoint!.Detail);
    }

    [Fact]
    public async Task GetVehicleById_IncludesTheManufacturer()
    {
        var actual = await GetAsync<VehicleDto>($"{ApiPaths.VehiclesFullPath}/1");

        Assert.Equal("SCANIA", actual.Manufacturer!.Description);
    }

    [Fact]
    public async Task GetScheduleById_IncludesRouteAndVehicle()
    {
        var actual = await GetAsync<ScheduleDto>($"{ApiPaths.SchedulesFullPath}/{DatabaseSeeder.ScheduleId}");

        Assert.Equal("Terminal de Transporte Norte", actual.Route!.OriginPoint!.Detail);
        Assert.Equal("ABC123", actual.Vehicle!.LicensePlate);
    }

    [Fact]
    public async Task GetReservationById_IncludesTheTripAndIdentificationType()
    {
        var actual = await GetAsync<ReservationDto>($"{ApiPaths.ReservationsFullPath}/{DatabaseSeeder.UpcomingReservationId}");

        Assert.Equal(DatabaseSeeder.ScheduleId, actual.Schedule!.ScheduleId);
        Assert.Equal("Bogotá", actual.Schedule!.Route!.DestinationPoint!.City!.Description);
        Assert.Equal("Cédula de ciudadanía", actual.IdentificationType!.Description);
    }

    [Fact]
    public async Task CreateRoute_ReturnsPointsAndCities()
    {
        // Act
        await AddAuthorization();
        var body = new RouteCreationDto { Description = "Bogotá - Medellín", OriginPointId = 2, DestinationPointId = 1 };
        var apiResponse = await Client.PostAsync(ApiPaths.RoutesFullPath,
            new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, MediaTypeNames.Application.Json));
        var actual = JsonConvert.DeserializeObject<JsonResponse<RouteDto>>(await apiResponse.Content.ReadAsStringAsync(), JsonSettings);

        // Assert
        Assert.Equal(HttpStatusCode.Created, apiResponse.StatusCode);
        Assert.Equal("Bogotá", actual!.Data!.OriginPoint!.City!.Description);
    }

    private async Task<T> GetAsync<T>(string url)
    {
        await AddAuthorization();
        var apiResponse = await Client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, apiResponse.StatusCode);
        var actual = JsonConvert.DeserializeObject<JsonResponse<T>>(await apiResponse.Content.ReadAsStringAsync(), JsonSettings);
        return actual!.Data!;
    }
}
