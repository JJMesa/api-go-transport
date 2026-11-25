using System.Net;
using System.Net.Mime;
using System.Text;
using GoTransport.Api.Component.Tests.Infrastructure;
using GoTransport.Application.Commons;
using GoTransport.Application.Dtos.City;
using GoTransport.Application.Wrappers;
using GoTransport.Domain.Entities.Bas;
using Newtonsoft.Json;

namespace GoTransport.Api.Component.Tests.TestCases;

public class CitiesControllerTests : ComponentTest
{
    public CitiesControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    #region GetAllByDepartmentAsync

    [Fact]
    public async Task GetAllByDepartmentAsync_Ok()
    {
        // Arrange
        int departmentId = DatabaseSeeder.AntioquiaDepartmentId;

        // Act
        await AddAuthorization();
        var apiResponse = await Client.GetAsync($"{ApiPaths.DepartmentsFullPath}/{departmentId}{ApiPaths.CitiesPath}");
        var jsonResponse = await apiResponse.Content.ReadAsStringAsync();
        var actual = JsonConvert.DeserializeObject<JsonResponse<List<CityDto>>>(jsonResponse, JsonSettings);

        // Assert
        Assert.Equal(HttpStatusCode.OK, apiResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, actual?.HttpCode);
        Assert.DoesNotContain(actual?.Data!, value => value.Department!.DepartmentId != departmentId);
        Assert.NotNull(apiResponse.Content);
    }

    [Fact]
    public async Task GetAllByDepartmentAsync_SortedByDescriptionAscending_Ok()
    {
        // Arrange
        int departmentId = DatabaseSeeder.AntioquiaDepartmentId;

        // Act
        await AddAuthorization();
        var apiResponse = await Client.GetAsync($"{ApiPaths.DepartmentsFullPath}/{departmentId}{ApiPaths.CitiesPath}");
        var jsonResponse = await apiResponse.Content.ReadAsStringAsync();
        var actual = JsonConvert.DeserializeObject<JsonResponse<List<CityDto>>>(jsonResponse, JsonSettings);
        var sortActual = actual?.Data?.OrderBy(x => x.Description).ToList();

        // Assert
        Assert.Equal(HttpStatusCode.OK, apiResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, actual?.HttpCode);
        Assert.NotNull(actual?.Data);
        Assert.NotNull(sortActual);
        Assert.DoesNotContain(actual?.Data!, value => value.Department!.DepartmentId != departmentId);
        Assert.Equal(sortActual, actual?.Data);
    }

    #endregion GetAllByDepartmentAsync

    #region GetAsync

    [Fact]
    public async Task GetAsync_Pagination_ItemPagination_Ok()
    {
        // Arrange
        int pageNumber = 1;
        int pageSize = 2;

        // Act
        await AddAuthorization();
        var apiResponse = await Client.GetAsync($"{ApiPaths.CitiesFullPath}?pageNumber={pageNumber}&pageSize={pageSize}");
        var jsonResponse = await apiResponse.Content.ReadAsStringAsync();
        var actual = JsonConvert.DeserializeObject<JsonPagedResponse<List<CityDto>>>(jsonResponse, JsonSettings);
        var paginationHeader = apiResponse.Headers.GetValues(Constants.PaginationHeader).First();
        var metada = JsonConvert.DeserializeObject<Metadata>(paginationHeader!, JsonSettings);

        // Assert
        Assert.Equal(HttpStatusCode.OK, apiResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, actual?.HttpCode);
        Assert.NotNull(actual?.Data);
        Assert.True(actual.Data.Count <= pageSize);
        Assert.Equal(pageNumber, metada!.CurrentPage);
        Assert.Equal(pageSize, metada!.PageSize);
    }

    [Theory]
    [InlineData(1000)]
    [InlineData(9999)]
    public async Task GetAsync_Pagination_PageSizeOutsideLimit_Ok(int pageSize)
    {
        // Arrange}
        int pageNumber = 1;

        // Act
        await AddAuthorization();
        var apiResponse = await Client.GetAsync($"{ApiPaths.CitiesFullPath}?pageNumber={pageNumber}&pageSize={pageSize}");
        var jsonResponse = await apiResponse.Content.ReadAsStringAsync();
        var actual = JsonConvert.DeserializeObject<JsonPagedResponse<List<CityDto>>>(jsonResponse, JsonSettings);
        var paginationHeader = apiResponse.Headers.GetValues(Constants.PaginationHeader).First();
        var metada = JsonConvert.DeserializeObject<Metadata>(paginationHeader!, JsonSettings);

        // Assert
        Assert.Equal(HttpStatusCode.OK, apiResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, actual?.HttpCode);
        Assert.NotNull(actual?.Data);
        Assert.True(actual.Data.Count <= Constants.MaximumRecordsPaged);
        Assert.Equal(pageNumber, metada!.CurrentPage);
        Assert.Equal(Constants.MaximumRecordsPaged, metada!.PageSize);
    }

    // The in-memory provider matches with case/accent-sensitive comparisons, so the criteria are
    // written to match the seeded descriptions exactly (SQL Server would match them accent-insensitively).
    [Theory]
    [InlineData("Medell")]
    [InlineData("Abejorral")]
    public async Task GetAsync_Parameters_WithFilter_Ok(string description)
    {
        // Act
        await AddAuthorization();
        var apiResponse = await Client.GetAsync($"{ApiPaths.CitiesFullPath}?searchCriteria={description}");
        var jsonResponse = await apiResponse.Content.ReadAsStringAsync();
        var actual = JsonConvert.DeserializeObject<JsonPagedResponse<List<CityDto>>>(jsonResponse, JsonSettings);

        // Assert
        Assert.Equal(HttpStatusCode.OK, apiResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, actual?.HttpCode);
        Assert.NotNull(actual?.Data);
        Assert.True(actual?.Data!.Count > 0);
    }

    [Theory]
    [InlineData("abcd")]
    [InlineData("fghi")]
    public async Task GetAsync_Parameters_WithFilterNotMatch_Ok(string description)
    {
        // Act
        await AddAuthorization();
        var apiResponse = await Client.GetAsync($"{ApiPaths.CitiesFullPath}?searchCriteria={description}");
        var jsonResponse = await apiResponse.Content.ReadAsStringAsync();
        var actual = JsonConvert.DeserializeObject<JsonPagedResponse<List<CityDto>>>(jsonResponse, JsonSettings);

        // Assert
        Assert.Equal(HttpStatusCode.OK, apiResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, actual?.HttpCode);
        Assert.True(actual?.Data!.Count == 0);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task GetAsync_Parameters_WithFilterNullOrEmpty_Ok(string? description)
    {
        // Act
        await AddAuthorization();
        var apiResponse = await Client.GetAsync($"{ApiPaths.CitiesFullPath}?searchCriteria={description}");
        var jsonResponse = await apiResponse.Content.ReadAsStringAsync();
        var actual = JsonConvert.DeserializeObject<JsonPagedResponse<List<CityDto>>>(jsonResponse, JsonSettings);

        // Assert
        Assert.Equal(HttpStatusCode.OK, apiResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, actual?.HttpCode);
        Assert.True(actual?.Data!.Count > 0);
    }

    [Fact]
    public async Task GetAsync_Parameters_WithDepartmentId_Ok()
    {
        // Arrange
        int departmentId = DatabaseSeeder.AntioquiaDepartmentId;

        // Act
        await AddAuthorization();
        var apiResponse = await Client.GetAsync($"{ApiPaths.CitiesFullPath}?departmentId={departmentId}");
        var jsonResponse = await apiResponse.Content.ReadAsStringAsync();
        var actual = JsonConvert.DeserializeObject<JsonPagedResponse<List<CityDto>>>(jsonResponse, JsonSettings);

        // Assert
        Assert.Equal(HttpStatusCode.OK, apiResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, actual?.HttpCode);
        Assert.True(actual?.Data!.Count > 0);
    }

    [Fact]
    public async Task GetAsync_Parameters_WithDepartmentId_CountsOnlyThatDepartment()
    {
        // Arrange: the seed has 2 Cities in Cundinamarca (Bogotá, Soacha) and 7 in total.
        const int cundinamarca = 2;

        // Act
        await AddAuthorization();
        var apiResponse = await Client.GetAsync($"{ApiPaths.CitiesFullPath}?departmentId={cundinamarca}");
        var metadata = JsonConvert.DeserializeObject<Metadata>(apiResponse.Headers.GetValues(Constants.PaginationHeader).Single());

        // Assert: the total count used to ignore the Department filter.
        Assert.Equal(2, metadata!.TotalItems);
        Assert.Equal(1, metadata.TotalPages);
    }

    [Fact]
    public async Task GetAsync_Parameters_WithNotExistsDepartmentId_Ok()
    {
        // Arrange
        int departmentId = DatabaseSeeder.UnknownId;

        // Act
        await AddAuthorization();
        var apiResponse = await Client.GetAsync($"{ApiPaths.CitiesFullPath}?departmentId={departmentId}");
        var jsonResponse = await apiResponse.Content.ReadAsStringAsync();
        var actual = JsonConvert.DeserializeObject<JsonPagedResponse<List<CityDto>>>(jsonResponse, JsonSettings);

        // Assert
        Assert.Equal(HttpStatusCode.OK, apiResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, actual?.HttpCode);
        Assert.True(actual?.Data!.Count == 0);
    }

    #endregion GetAsync

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_Ok()
    {
        // Arrange
        int cityId = DatabaseSeeder.MedellinCityId;

        // Act
        await AddAuthorization();
        var apiResponse = await Client.GetAsync($"{ApiPaths.CitiesFullPath}/{cityId}");
        var jsonResponse = await apiResponse.Content.ReadAsStringAsync();
        var actual = JsonConvert.DeserializeObject<JsonResponse<CityDto>>(jsonResponse, JsonSettings);

        // Assert
        Assert.Equal(HttpStatusCode.OK, apiResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, actual?.HttpCode);
        Assert.NotNull(actual?.Data);
        Assert.Equal(cityId, actual?.Data.CityId);
    }

    [Fact]
    public async Task GetByIdAsync_WithNotExistsCityId_NotFound()
    {
        // Arrange
        int cityId = DatabaseSeeder.UnknownId;

        // Act
        await AddAuthorization();
        var apiResponse = await Client.GetAsync($"{ApiPaths.CitiesFullPath}/{cityId}");
        var jsonResponse = await apiResponse.Content.ReadAsStringAsync();
        var actual = JsonConvert.DeserializeObject<JsonResponse<CityDto>>(jsonResponse, JsonSettings);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, apiResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, actual?.HttpCode);
        Assert.Null(actual?.Data);
    }

    [Theory]
    [InlineData("1B")]
    [InlineData("B1")]
    [InlineData("1,1")]
    [InlineData("1.1")]
    public async Task GetByIdAsync_WithInvalidCityId_NotFound(string cityId)
    {
        // Act
        await AddAuthorization();
        var apiResponse = await Client.GetAsync($"{ApiPaths.CitiesFullPath}/{cityId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, apiResponse.StatusCode);
    }

    #endregion GetByIdAsync

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_Ok()
    {
        // Arrange
        var cityCreationDto = NewCity();
        var contentRequest = new StringContent(JsonConvert.SerializeObject(cityCreationDto), Encoding.UTF8, MediaTypeNames.Application.Json);

        // Act
        await AddAuthorization();
        var apiResponseCreation = await Client.PostAsync(ApiPaths.CitiesFullPath, contentRequest);
        var jsonResponseCreation = await apiResponseCreation.Content.ReadAsStringAsync();
        var actual = JsonConvert.DeserializeObject<JsonResponse<CityDto>>(jsonResponseCreation, JsonSettings);
        var apiResponseDelete = await Client.DeleteAsync($"{ApiPaths.CitiesFullPath}/{actual!.Data!.CityId}");

        // Assert
        Assert.Equal(HttpStatusCode.Created, apiResponseCreation.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, apiResponseDelete.StatusCode);
        Assert.Equal(HttpStatusCode.Created, actual?.HttpCode);
        Assert.NotNull(actual?.Data);
        Assert.Equal(cityCreationDto.Description, actual.Data!.Description);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidDescription_BadRequest()
    {
        // Arrange
        var cityCreationDto = NewCity();
        cityCreationDto.Description = MoreThan128Characters;
        var contentRequest = new StringContent(JsonConvert.SerializeObject(cityCreationDto), Encoding.UTF8, MediaTypeNames.Application.Json);

        // Act
        await AddAuthorization();
        var apiResponse = await Client.PostAsync(ApiPaths.CitiesFullPath, contentRequest);
        var jsonResponse = await apiResponse.Content.ReadAsStringAsync();
        var actual = JsonConvert.DeserializeObject<JsonResponse<CityDto>>(jsonResponse, JsonSettings);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, apiResponse.StatusCode); ;
        Assert.Equal(HttpStatusCode.BadRequest, actual?.HttpCode);
        Assert.Null(actual?.Data);
    }

    [Fact]
    public async Task CreateAsync_WithoutRequiredProperties_BadRequest()
    {
        // Arrange
        var cityCreationDto = NewCity();
        cityCreationDto.Description = "";
        var contentRequest = new StringContent(JsonConvert.SerializeObject(cityCreationDto), Encoding.UTF8, MediaTypeNames.Application.Json);

        // Act
        await AddAuthorization();
        var apiResponse = await Client.PostAsync(ApiPaths.CitiesFullPath, contentRequest);
        var jsonResponse = await apiResponse.Content.ReadAsStringAsync();
        var actual = JsonConvert.DeserializeObject<JsonResponse<CityDto>>(jsonResponse, JsonSettings);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, apiResponse.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, actual?.HttpCode);
        Assert.Null(actual?.Data);
    }

    #endregion CreateAsync

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_Ok()
    {
        #region Create

        // Arrange
        var cityCreationDto = NewCity();
        var contentRequestCreated = new StringContent(JsonConvert.SerializeObject(cityCreationDto), Encoding.UTF8, MediaTypeNames.Application.Json);

        // Act
        await AddAuthorization();
        var apiResponseCreation = await Client.PostAsync(ApiPaths.CitiesFullPath, contentRequestCreated);
        var jsonResponseCreation = await apiResponseCreation.Content.ReadAsStringAsync();
        var actualCreation = JsonConvert.DeserializeObject<JsonResponse<CityDto>>(jsonResponseCreation, JsonSettings);

        // Assert
        Assert.Equal(HttpStatusCode.Created, actualCreation!.HttpCode);

        #endregion Create

        #region Update

        // Arrange
        var cityUpdateDto = CityUpdate(actualCreation.Data!.CityId);
        var contentRequestUpdated = new StringContent(JsonConvert.SerializeObject(cityUpdateDto), Encoding.UTF8, MediaTypeNames.Application.Json);

        //Act
        await AddAuthorization();
        var apiResponseUpdate = await Client.PutAsync($"{ApiPaths.CitiesFullPath}/{actualCreation.Data!.CityId}", contentRequestUpdated);
        var jsonResponseUpdate = await apiResponseUpdate.Content.ReadAsStringAsync();
        var actualUpdate = JsonConvert.DeserializeObject<JsonResponse<CityDto>>(jsonResponseUpdate, JsonSettings);

        #endregion Update

        #region Delete

        // Act
        var apiResponseDelete = await Client.DeleteAsync($"{ApiPaths.CitiesFullPath}/{actualCreation.Data!.CityId}");

        #endregion Delete

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, apiResponseDelete.StatusCode);
        Assert.Equal(HttpStatusCode.OK, apiResponseUpdate.StatusCode);
        Assert.Equal(HttpStatusCode.Created, actualCreation.HttpCode);
        Assert.NotNull(actualUpdate?.Data);
        Assert.Equal(cityUpdateDto.Description, actualUpdate.Data?.Description);
    }

    [Fact]
    public async Task UpdateAsync_AsSignedInUser_StampsUpdateUser()
    {
        // Arrange — the seeded City was created by the system user
        int cityId = DatabaseSeeder.MedellinCityId;
        var cityUpdateDto = CityUpdate(cityId);
        var contentRequest = new StringContent(JsonConvert.SerializeObject(cityUpdateDto, JsonSettings), Encoding.UTF8, MediaTypeNames.Application.Json);

        // Act
        await AddAuthorization();
        var apiResponse = await Client.PutAsync($"{ApiPaths.CitiesFullPath}/{cityId}", contentRequest);
        var city = FindInDatabase<City>(cityId);

        // Assert
        Assert.Equal(HttpStatusCode.OK, apiResponse.StatusCode);
        Assert.Equal(Constants.ApplicationUser, city?.CreationUser);
        Assert.Equal(TestAuthHandler.TestUserId, city?.UpdateUser);
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidDescription_BadRequest()
    {
        //Arrange
        int cityId = DatabaseSeeder.MedellinCityId;
        var cityUpdateDto = CityUpdate(cityId);
        cityUpdateDto.Description = MoreThan128Characters;
        var contentRequest = new StringContent(JsonConvert.SerializeObject(cityUpdateDto, JsonSettings), Encoding.UTF8, MediaTypeNames.Application.Json);

        //Act
        await AddAuthorization();
        var apiResponse = await Client.PutAsync($"{ApiPaths.CitiesFullPath}/{cityId}", contentRequest);
        var jsonResponse = await apiResponse.Content.ReadAsStringAsync();
        var actual = JsonConvert.DeserializeObject<JsonResponse<CityDto>>(jsonResponse, JsonSettings);

        //Assert
        Assert.Equal(HttpStatusCode.BadRequest, apiResponse.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, actual?.HttpCode);
        Assert.Null(actual?.Data);
    }

    [Fact]
    public async Task UpdateAsync_WithoutRequiredProperties_BadRequest()
    {
        //Arrange
        int cityId = DatabaseSeeder.MedellinCityId;
        var cityUpdateDto = CityUpdate(cityId);
        cityUpdateDto.Description = "";
        var contentRequest = new StringContent(JsonConvert.SerializeObject(cityUpdateDto, JsonSettings), Encoding.UTF8, MediaTypeNames.Application.Json);

        //Act
        await AddAuthorization();
        var apiResponse = await Client.PutAsync($"{ApiPaths.CitiesFullPath}/{cityId}", contentRequest);
        var jsonResponse = await apiResponse.Content.ReadAsStringAsync();
        var actual = JsonConvert.DeserializeObject<JsonResponse<CityDto>>(jsonResponse, JsonSettings);

        //Assert
        Assert.Equal(HttpStatusCode.BadRequest, apiResponse.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, actual?.HttpCode);
        Assert.Null(actual?.Data);
    }

    [Fact]
    public async Task UpdateAsync_WithIdDifferentToBodyId_BadRequest()
    {
        //Arrange
        var cityUpdateDto = CityUpdate(DatabaseSeeder.MedellinCityId);
        var contentRequest = new StringContent(JsonConvert.SerializeObject(cityUpdateDto, JsonSettings), Encoding.UTF8, MediaTypeNames.Application.Json);

        //Act
        await AddAuthorization();
        var apiResponse = await Client.PutAsync($"{ApiPaths.CitiesFullPath}/{DatabaseSeeder.UnknownId}", contentRequest);
        var jsonResponse = await apiResponse.Content.ReadAsStringAsync();
        var actual = JsonConvert.DeserializeObject<JsonResponse<CityDto>>(jsonResponse, JsonSettings);

        //Assert
        Assert.Equal(HttpStatusCode.BadRequest, apiResponse.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, actual?.HttpCode);
        Assert.Null(actual?.Data);
    }

    [Theory]
    [InlineData("1B")]
    [InlineData("B1")]
    [InlineData("1,1")]
    [InlineData("1.1")]
    public async Task UpdateAsync_WithInvalidCityId_NotFound(string id)
    {
        //Arrange
        var cityUpdateDto = CityUpdate(cityId: 9999);
        var contentRequest = new StringContent(JsonConvert.SerializeObject(cityUpdateDto, JsonSettings), Encoding.UTF8, MediaTypeNames.Application.Json);

        //Act
        await AddAuthorization();
        var apiResponse = await Client.PutAsync($"{ApiPaths.CitiesFullPath}/{id}", contentRequest);
        var jsonResponse = await apiResponse.Content.ReadAsStringAsync();
        var actual = JsonConvert.DeserializeObject<JsonResponse<CityDto>>(jsonResponse, JsonSettings);

        //Assert
        Assert.Equal(HttpStatusCode.NotFound, apiResponse.StatusCode);
        Assert.Null(actual?.Data);
    }

    #endregion UpdateAsync

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_NoContent()
    {
        #region Create

        // Arrange
        var cityTobeCreated = NewCity();
        var contentRequest = new StringContent(JsonConvert.SerializeObject(cityTobeCreated), Encoding.UTF8, MediaTypeNames.Application.Json);

        // Act
        await AddAuthorization();
        var apiResponse = await Client.PostAsync(ApiPaths.CitiesFullPath, contentRequest);
        var jsonResponse = await apiResponse.Content.ReadAsStringAsync();
        var actual = JsonConvert.DeserializeObject<JsonResponse<CityDto>>(jsonResponse, JsonSettings);

        // Assert
        Assert.Equal(HttpStatusCode.Created, apiResponse.StatusCode);

        #endregion Create

        #region Delete

        // Act
        var apiResponseDelete = await Client.DeleteAsync($"{ApiPaths.CitiesFullPath}/{actual!.Data!.CityId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, apiResponseDelete.StatusCode);

        #endregion Delete
    }

    [Theory]
    [InlineData("1B")]
    [InlineData("B1")]
    [InlineData("1,1")]
    [InlineData("1.1")]
    [InlineData("9999")]
    public async Task DeleteAsync_WithInvalidCityId_NotFound(string id)
    {
        // Act
        await AddAuthorization();
        var apiResponse = await Client.DeleteAsync($"{ApiPaths.CitiesFullPath}/{id}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, apiResponse.StatusCode);
    }

    #endregion DeleteAsync

    private const string MoreThan128Characters = "Lorem ipsum dolor sit amet consectetur adipiscing elit potenti ultricies sapien, cubilia scelerisque augue feugiat proin nam curabitur";

    private static CityCreationDto NewCity(int departmentId = DatabaseSeeder.AntioquiaDepartmentId) => new()
    {
        Description = "Prueba Creación",
        DepartmentId = departmentId
    };

    private static CityUpdateDto CityUpdate(int cityId = DatabaseSeeder.MedellinCityId, int departmentId = DatabaseSeeder.AntioquiaDepartmentId) => new()
    {
        CityId = cityId,
        Description = "Prueba Actualización",
        DepartmentId = departmentId,
        IsActive = true
    };
}
