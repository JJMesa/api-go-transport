using System.Net;
using System.Net.Mime;
using System.Text;
using GoTransport.Api.Component.Tests.Infrastructure;
using GoTransport.Application.Wrappers;
using Newtonsoft.Json;
using GoTransport.Application.Dtos.Vehicle;

namespace GoTransport.Api.Component.Tests.TestCases;

public class VehiclesControllerTests : ComponentTest
{
    public VehiclesControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateAsync_Created_WithLocationAndManufacturer()
    {
        // Act
        await AddAuthorization();
        var apiResponse = await Client.PostAsync(ApiPaths.VehiclesFullPath,
            Json(new VehicleCreationDto { ManufacturerId = 1, LicensePlate = "XYZ987", Model = 2024, Capacity = 40 }));
        var actual = await ReadAsync<VehicleDto>(apiResponse);

        // Assert
        Assert.Equal(HttpStatusCode.Created, apiResponse.StatusCode);
        Assert.Equal("SCANIA", actual.Data!.Manufacturer!.Description);
        Assert.Equal($"/api/v1/vehicles/{actual.Data.VehicleId}", apiResponse.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task CreateAsync_WithASeededLicensePlate_Conflict()
    {
        // Act
        await AddAuthorization();
        var apiResponse = await Client.PostAsync(ApiPaths.VehiclesFullPath,
            Json(new VehicleCreationDto { ManufacturerId = 1, LicensePlate = "ABC123", Model = 2024, Capacity = 40 }));

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, apiResponse.StatusCode);
    }

    [Fact]
    public async Task UpdateAsync_Ok()
    {
        // Act
        await AddAuthorization();
        var apiResponse = await Client.PutAsync($"{ApiPaths.VehiclesFullPath}/1", Json(new VehicleUpdateDto { VehicleId = 1, Capacity = 36 }));
        var actual = await ReadAsync<VehicleDto>(apiResponse);

        // Assert
        Assert.Equal(HttpStatusCode.OK, apiResponse.StatusCode);
        Assert.Equal(36, actual.Data!.Capacity);
    }

    private static StringContent Json(object body) =>
        new(JsonConvert.SerializeObject(body), Encoding.UTF8, MediaTypeNames.Application.Json);

    private async Task<JsonResponse<T>> ReadAsync<T>(HttpResponseMessage response) =>
        JsonConvert.DeserializeObject<JsonResponse<T>>(await response.Content.ReadAsStringAsync(), JsonSettings)!;
}
