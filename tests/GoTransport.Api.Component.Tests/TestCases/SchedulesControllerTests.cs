using System.Net;
using System.Net.Mime;
using System.Text;
using GoTransport.Api.Component.Tests.Infrastructure;
using GoTransport.Api.Test.Utilities.Commons;
using GoTransport.Application.Wrappers;
using Newtonsoft.Json;
using GoTransport.Application.Dtos.Schedule;

namespace GoTransport.Api.Component.Tests.TestCases;

public class SchedulesControllerTests : ComponentTest
{
    public SchedulesControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateAsync_Created_WithLocationAndDuration()
    {
        // Arrange: an overnight Schedule on the seeded Route and Vehicle
        var schedule = new ScheduleCreationDto { DepartureTime = new TimeSpan(22, 0, 0), ArrivalTime = new TimeSpan(6, 0, 0), RouteId = 1, VehicleId = 1 };

        // Act
        await AddAuthorization();
        var apiResponse = await Client.PostAsync(ApiPaths.SchedulesFullPath, Json(schedule));
        var actual = await ReadAsync<ScheduleDto>(apiResponse);

        // Assert
        Assert.Equal(HttpStatusCode.Created, apiResponse.StatusCode);
        Assert.Equal(TimeSpan.FromHours(8), actual.Data!.Duration);
        Assert.Equal($"/api/v1/schedules/{actual.Data.ScheduleId}", apiResponse.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task UpdateAsync_Ok()
    {
        // Arrange
        var update = new ScheduleUpdateDto { ScheduleId = DatabaseSeeder.ScheduleId, DepartureTime = new TimeSpan(7, 0, 0), ArrivalTime = new TimeSpan(16, 30, 0), RouteId = 1, VehicleId = 1, IsActive = true };

        // Act
        await AddAuthorization();
        var apiResponse = await Client.PutAsync($"{ApiPaths.SchedulesFullPath}/{DatabaseSeeder.ScheduleId}", Json(update));
        var actual = await ReadAsync<ScheduleDto>(apiResponse);

        // Assert
        Assert.Equal(HttpStatusCode.OK, apiResponse.StatusCode);
        Assert.Equal(new TimeSpan(9, 30, 0), actual.Data!.Duration);
    }

    private static StringContent Json(object body) =>
        new(JsonConvert.SerializeObject(body), Encoding.UTF8, MediaTypeNames.Application.Json);

    private async Task<JsonResponse<T>> ReadAsync<T>(HttpResponseMessage response) =>
        JsonConvert.DeserializeObject<JsonResponse<T>>(await response.Content.ReadAsStringAsync(), JsonSettings)!;
}
