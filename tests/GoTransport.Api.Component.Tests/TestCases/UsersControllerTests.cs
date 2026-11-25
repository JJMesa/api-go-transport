using System.Net;
using System.Net.Mime;
using System.Text;
using GoTransport.Api.Component.Tests.Infrastructure;
using GoTransport.Api.Test.Utilities.Commons;
using GoTransport.Application.Wrappers;
using Newtonsoft.Json;
using GoTransport.Application.Dtos.User;

namespace GoTransport.Api.Component.Tests.TestCases;

public class UsersControllerTests : ComponentTest
{
    public UsersControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Create_Created_WithLocation()
    {
        // Act
        await AddAuthorization();
        var apiResponse = await Client.PostAsync(ApiPaths.UsersFullPath, Json(NewOperator()));
        var actual = await ReadAsync<UserDto>(apiResponse);

        // Assert
        Assert.Equal(HttpStatusCode.Created, apiResponse.StatusCode);
        Assert.Equal("ana@gotransport.test", actual.Data!.Email);
        Assert.Equal($"/api/v1/users/{actual.Data.UserId}", apiResponse.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Create_WithAnUnknownRole_BadRequest()
    {
        // Arrange
        var user = NewOperator();
        user.RoleId = 99;

        // Act
        await AddAuthorization();
        var apiResponse = await Client.PostAsync(ApiPaths.UsersFullPath, Json(user));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, apiResponse.StatusCode);
    }

    [Fact]
    public async Task Update_DeactivatesAndChangesTheRole_Ok()
    {
        // Arrange
        await AddAuthorization();
        var created = await ReadAsync<UserDto>(await Client.PostAsync(ApiPaths.UsersFullPath, Json(NewOperator())));
        var update = new UserUpdateDto { UserId = created.Data!.UserId, FirstName = "Ana", LastName = "Ruiz", IsActive = false, RoleId = DatabaseSeeder.AdministratorRoleId };

        // Act
        var apiResponse = await Client.PutAsync($"{ApiPaths.UsersFullPath}/{created.Data.UserId}", Json(update));
        var actual = await ReadAsync<UserDto>(apiResponse);

        // Assert
        Assert.Equal(HttpStatusCode.OK, apiResponse.StatusCode);
        Assert.False(actual.Data!.IsActive);
    }

    private static UserCreationDto NewOperator() => new()
    {
        FirstName = "Ana",
        LastName = "Ruiz",
        Email = "ana@gotransport.test",
        Password = "Transporte-2026",
        RoleId = DatabaseSeeder.OperatorRoleId
    };

    private static StringContent Json(object body) =>
        new(JsonConvert.SerializeObject(body), Encoding.UTF8, MediaTypeNames.Application.Json);

    private async Task<JsonResponse<T>> ReadAsync<T>(HttpResponseMessage response) =>
        JsonConvert.DeserializeObject<JsonResponse<T>>(await response.Content.ReadAsStringAsync(), JsonSettings)!;
}
