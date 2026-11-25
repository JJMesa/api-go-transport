using System.Net;
using System.Net.Mime;
using System.Text;
using GoTransport.Api.Component.Tests.Infrastructure;
using GoTransport.Application.Wrappers;
using Newtonsoft.Json;
using GoTransport.Application.Dtos.Department;

namespace GoTransport.Api.Component.Tests.TestCases;

public class DepartmentsControllerTests : ComponentTest
{
    public DepartmentsControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateAsync_Created_WithLocation()
    {
        // Act
        await AddAuthorization();
        var apiResponse = await Client.PostAsync(ApiPaths.DepartmentsFullPath, Json(new DepartmentCreationDto { Description = "Boyacá" }));
        var actual = await ReadAsync<DepartmentDto>(apiResponse);

        // Assert
        Assert.Equal(HttpStatusCode.Created, apiResponse.StatusCode);
        Assert.Equal("Boyacá", actual.Data!.Description);
        Assert.Equal($"/api/v1/departments/{actual.Data.DepartmentId}", apiResponse.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task UpdateAsync_Ok()
    {
        // Act
        await AddAuthorization();
        var apiResponse = await Client.PutAsync($"{ApiPaths.DepartmentsFullPath}/2",
            Json(new DepartmentUpdateDto { DepartmentId = 2, Description = "Cundinamarca y Bogotá", IsActive = true }));
        var actual = await ReadAsync<DepartmentDto>(apiResponse);

        // Assert
        Assert.Equal(HttpStatusCode.OK, apiResponse.StatusCode);
        Assert.Equal("Cundinamarca y Bogotá", actual.Data!.Description);
    }

    private static StringContent Json(object body) =>
        new(JsonConvert.SerializeObject(body), Encoding.UTF8, MediaTypeNames.Application.Json);

    private async Task<JsonResponse<T>> ReadAsync<T>(HttpResponseMessage response) =>
        JsonConvert.DeserializeObject<JsonResponse<T>>(await response.Content.ReadAsStringAsync(), JsonSettings)!;
}
