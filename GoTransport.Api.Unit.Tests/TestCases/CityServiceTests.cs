using System.Net;
using AutoMapper;
using GoTransport.Api.Test.Utilities.Commons;
using GoTransport.Api.Test.Utilities.Mothers;
using GoTransport.Application.Commons;
using GoTransport.Application.Dtos.City;
using GoTransport.Application.Dtos.Department;
using GoTransport.Application.Enums;
using GoTransport.Application.Interfaces;
using GoTransport.Application.Interfaces.Base;
using GoTransport.Application.Mappings;
using GoTransport.Application.Parameters;
using GoTransport.Application.Services;
using GoTransport.Application.Specifications.Cities;
using GoTransport.Domain.Entities.Bas;
using Moq;

namespace GoTransport.Api.Unit.Tests.TestCases;

public class CityServiceTests
{
    private readonly Mock<IRepository<City>> _cityRepositoryMock;
    private readonly IMapper _mapper;
    private readonly ICityService _cityService;

    public CityServiceTests()
    {
        _cityRepositoryMock = new Mock<IRepository<City>>();
        _mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile(new CityMappingProfile())));
        _cityService = new CityService(_cityRepositoryMock.Object, _mapper);
    }

    #region DataSeed

    private static List<City> CitiesList()
    {
        return new List<City> {
            new() { CityId = 1, Description = "city 1", DepartmentId = 1 },
            new() { CityId = 2, Description = "city 2", DepartmentId = 1 },
            new() { CityId = 3, Description = "city 3", DepartmentId = 2 }
        };
    }

    private static List<CityDto> CitiesDtoList()
    {
        return new List<CityDto> {
            new() { CityId = 1, Description = "city 1", Department = new DepartmentDto {  DepartmentId = 1, Description = "department 1" } },
            new() { CityId = 2, Description = "city 2", Department = new DepartmentDto {  DepartmentId = 1, Description = "department 1" } },
            new() { CityId = 3, Description = "city 3", Department = new DepartmentDto {  DepartmentId = 2, Description = "department 2" } }
        };
    }

    private static List<CityCreationDto> CitiesCreationDtoList()
    {
        return new List<CityCreationDto> {
            new() { Description = "city 1", DepartmentId = 1 },
            new() { Description = "city 2", DepartmentId = 1 },
            new() { Description = "city 3", DepartmentId = 2 }
        };
    }

    private static List<CityUpdateDto> CitiesUpdateDtoList()
    {
        return new List<CityUpdateDto> {
            new() { CityId = 1, Description = "city 1 updated", DepartmentId = 1 },
            new() { CityId = 2, Description = "city 2 updated", DepartmentId = 1 },
            new() { CityId = 3, Description = "city 3 updated", DepartmentId = 2 }
        };
    }

    #endregion DataSeed

    #region GetAllByDepartmentAsync

    [Fact]
    public async Task GetAllByDepartmentAsync_ShouldReturnCitiesForValidDepartment()
    {
        // Arrange
        var departmentId = Utils.DefaultId;
        var cityData = CitiesList();
        var cityDtoData = CitiesDtoList();

        _cityRepositoryMock.Setup(x => x.ListAsync(It.IsAny<CitySpecification>(), CancellationToken.None)).ReturnsAsync(cityData);

        // Act
        var actual = await _cityService.GetAllByDepartmentAsync(departmentId, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, actual.HttpCode);
        Assert.True(actual.Data?.Count() > 0);
        _cityRepositoryMock.Verify(x => x.ListAsync(It.IsAny<CitySpecification>(), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task GetAllByDepartmentAsync_ShouldReturnEmptyListForNonexistentDepartment()
    {
        // Arrange
        var departmentId = Utils.IdDoesNotExist;
        var cityData = new List<City>();
        var cityDtoData = new List<CityDto>();

        _cityRepositoryMock.Setup(x => x.ListAsync(It.IsAny<CitySpecification>(), CancellationToken.None)).ReturnsAsync(cityData);

        // Act
        var actual = await _cityService.GetAllByDepartmentAsync(departmentId, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, actual.HttpCode);
        Assert.True(actual.Data?.Count() == 0);
        _cityRepositoryMock.Verify(x => x.ListAsync(It.IsAny<CitySpecification>(), CancellationToken.None), Times.Once);
    }

    #endregion GetAllByDepartmentAsync

    #region GetAsync

    [Fact]
    public async Task GetAsync_ShouldReturnCitiesForValidParameters()
    {
        // Arrange
        var cityDtoData = CitiesDtoList();

        _cityRepositoryMock.Setup(x => x.ListAsync(It.IsAny<PagedCitySpecification>(), CancellationToken.None)).ReturnsAsync(cityDtoData);
        _cityRepositoryMock.Setup(x => x.CountAsync(It.IsAny<CitySpecification>(), CancellationToken.None)).ReturnsAsync(cityDtoData.Count);

        // Act
        var actual = await _cityService.GetAsync(new CityParameters(), CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, actual.HttpCode);
        Assert.True(actual.Data?.Count() > 0);
        _cityRepositoryMock.Verify(x => x.ListAsync(It.IsAny<PagedCitySpecification>(), CancellationToken.None), Times.Once);
        _cityRepositoryMock.Verify(x => x.CountAsync(It.IsAny<CitySpecification>(), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnEmptyListForValidParameters()
    {
        // Arrange
        var cityDtoData = new List<CityDto>();

        _cityRepositoryMock.Setup(x => x.ListAsync(It.IsAny<PagedCitySpecification>(), CancellationToken.None)).ReturnsAsync(cityDtoData);
        _cityRepositoryMock.Setup(x => x.CountAsync(It.IsAny<CitySpecification>(), CancellationToken.None)).ReturnsAsync(cityDtoData.Count);

        var parameters = CityBuilderMother.CityParameters();

        // Act
        var actual = await _cityService.GetAsync(parameters, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, actual.HttpCode);
        Assert.True(actual.Data?.Count() == 0);
        _cityRepositoryMock.Verify(x => x.ListAsync(It.IsAny<PagedCitySpecification>(), CancellationToken.None), Times.Once);
        _cityRepositoryMock.Verify(x => x.CountAsync(It.IsAny<CitySpecification>(), CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task GetAsync_ShouldReturnCitiesForValidDepartment(int departmentId)
    {
        // Arrange
        var cityDtoData = CitiesDtoList().Where(x => x.Department.DepartmentId == departmentId).ToList();

        _cityRepositoryMock.Setup(x => x.ListAsync(It.IsAny<PagedCitySpecification>(), CancellationToken.None)).ReturnsAsync(cityDtoData);
        _cityRepositoryMock.Setup(x => x.CountAsync(It.IsAny<CitySpecification>(), CancellationToken.None)).ReturnsAsync(cityDtoData.Count);

        var parameters = CityBuilderMother.CityParameters(departmentId: departmentId);

        // Act
        var actual = await _cityService.GetAsync(parameters, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, actual.HttpCode);
        Assert.True(actual.Data?.Count() > 0);
        _cityRepositoryMock.Verify(x => x.ListAsync(It.IsAny<PagedCitySpecification>(), CancellationToken.None), Times.Once);
        _cityRepositoryMock.Verify(x => x.CountAsync(It.IsAny<CitySpecification>(), CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(9999)]
    public async Task GetAsync_ShouldReturnEmptyListForNonexistentDepartment(int departmentId)
    {
        // Arrange
        var cityDtoData = new List<CityDto>();

        _cityRepositoryMock.Setup(x => x.ListAsync(It.IsAny<PagedCitySpecification>(), CancellationToken.None)).ReturnsAsync(cityDtoData);
        _cityRepositoryMock.Setup(x => x.CountAsync(It.IsAny<CitySpecification>(), CancellationToken.None)).ReturnsAsync(cityDtoData.Count);

        var parameters = CityBuilderMother.CityParameters(departmentId: departmentId);

        // Act
        var actual = await _cityService.GetAsync(parameters, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, actual.HttpCode);
        Assert.True(actual.Data?.Count() == 0);
        _cityRepositoryMock.Verify(x => x.ListAsync(It.IsAny<PagedCitySpecification>(), CancellationToken.None), Times.Once);
        _cityRepositoryMock.Verify(x => x.CountAsync(It.IsAny<CitySpecification>(), CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData("city 1")]
    [InlineData("city 2")]
    [InlineData("city 3")]
    public async Task GetAsync_ShouldReturnMatchingCitiesForSearchCriteria(string description)
    {
        // Arrange
        var cityDtoData = CitiesDtoList().Where(x => x.Description == description).ToList();

        _cityRepositoryMock.Setup(x => x.ListAsync(It.IsAny<PagedCitySpecification>(), CancellationToken.None)).ReturnsAsync(cityDtoData);
        _cityRepositoryMock.Setup(x => x.CountAsync(It.IsAny<CitySpecification>(), CancellationToken.None)).ReturnsAsync(cityDtoData.Count);

        var parameters = CityBuilderMother.CityParameters(search: description);

        // Act
        var actual = await _cityService.GetAsync(parameters, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, actual.HttpCode);
        Assert.True(actual.Data?.Count() > 0);
        _cityRepositoryMock.Verify(x => x.ListAsync(It.IsAny<PagedCitySpecification>(), CancellationToken.None), Times.Once);
        _cityRepositoryMock.Verify(x => x.CountAsync(It.IsAny<CitySpecification>(), CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData("ABCD-1234")]
    [InlineData("EFGH-5678")]
    [InlineData("X6H73J8K6")]
    public async Task GetAsync_ShouldReturnEmptyListForNonMatchingSearchCriteria(string description)
    {
        // Arrange
        var cityDtoData = new List<CityDto>();

        _cityRepositoryMock.Setup(x => x.ListAsync(It.IsAny<PagedCitySpecification>(), CancellationToken.None)).ReturnsAsync(cityDtoData);
        _cityRepositoryMock.Setup(x => x.CountAsync(It.IsAny<CitySpecification>(), CancellationToken.None)).ReturnsAsync(cityDtoData.Count);

        var parameters = CityBuilderMother.CityParameters(search: description);

        // Act
        var actual = await _cityService.GetAsync(parameters, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, actual.HttpCode);
        Assert.True(actual.Data?.Count() == 0);
        _cityRepositoryMock.Verify(x => x.ListAsync(It.IsAny<PagedCitySpecification>(), CancellationToken.None), Times.Once);
        _cityRepositoryMock.Verify(x => x.CountAsync(It.IsAny<CitySpecification>(), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task CityService_GetAsync_ShouldReturnCitiesSortedAscendingk()
    {
        // Arrange
        var cityDtoData = CitiesDtoList().OrderBy(x => x.Description).ToList();

        _cityRepositoryMock.Setup(x => x.ListAsync(It.IsAny<PagedCitySpecification>(), CancellationToken.None)).ReturnsAsync(cityDtoData);
        _cityRepositoryMock.Setup(x => x.CountAsync(It.IsAny<CitySpecification>(), CancellationToken.None)).ReturnsAsync(cityDtoData.Count);

        var parameters = CityBuilderMother.CityParameters(sort: SortDirection.Ascending);

        // Act
        var actual = await _cityService.GetAsync(parameters, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, actual.HttpCode);
        Assert.True(actual.Data?.Count() > 0);
        Assert.Equal(cityDtoData!, actual.Data);
        _cityRepositoryMock.Verify(x => x.ListAsync(It.IsAny<PagedCitySpecification>(), CancellationToken.None), Times.Once);
        _cityRepositoryMock.Verify(x => x.CountAsync(It.IsAny<CitySpecification>(), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task CityService_GetAsync_ShouldReturnCitiesSortedDescending()
    {
        // Arrange
        var cityDtoData = CitiesDtoList().OrderByDescending(x => x.Description).ToList();

        _cityRepositoryMock.Setup(x => x.ListAsync(It.IsAny<PagedCitySpecification>(), CancellationToken.None)).ReturnsAsync(cityDtoData);
        _cityRepositoryMock.Setup(x => x.CountAsync(It.IsAny<CitySpecification>(), CancellationToken.None)).ReturnsAsync(cityDtoData.Count);

        var parameters = CityBuilderMother.CityParameters(sort: SortDirection.Descending);

        // Act
        var actual = await _cityService.GetAsync(parameters, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, actual.HttpCode);
        Assert.True(actual.Data?.Count() > 0);
        Assert.Equal(cityDtoData, actual.Data);
        _cityRepositoryMock.Verify(x => x.ListAsync(It.IsAny<PagedCitySpecification>(), CancellationToken.None), Times.Once);
        _cityRepositoryMock.Verify(x => x.CountAsync(It.IsAny<CitySpecification>(), CancellationToken.None), Times.Once);
    }

    #endregion GetAsync

    #region GetByIdAsync

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task GetByIdAsync_ShouldReturnCityForValidId(int cityId)
    {
        // Arrange
        var cityData = CitiesList().FirstOrDefault(x => x.CityId == cityId);
        var cityDtoData = CitiesDtoList().FirstOrDefault(x => x.CityId == cityId);

        _cityRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<CitySpecification>(), CancellationToken.None)).ReturnsAsync(cityData);

        // Act
        var actual = await _cityService.GetByIdAsync(cityId, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, actual.HttpCode);
        Assert.Equal(cityDtoData?.Description, actual.Data!.Description);
        _cityRepositoryMock.Verify(x => x.FirstOrDefaultAsync(It.IsAny<CitySpecification>(), CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    public async Task GetByIdAsync_ShouldReturnNotFoundForInvalidId(int cityId)
    {
        // Arrange
        var cityData = CitiesList().FirstOrDefault(x => x.CityId == cityId);

        _cityRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<CitySpecification>(), CancellationToken.None)).ReturnsAsync(cityData);

        // Act
        var actual = await _cityService.GetByIdAsync(cityId, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, actual.HttpCode);
        Assert.Null(actual.Data);
        _cityRepositoryMock.Verify(x => x.FirstOrDefaultAsync(It.IsAny<CitySpecification>(), CancellationToken.None), Times.Once);
    }

    #endregion GetByIdAsync

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_ShouldCreateCityAndReturnOk()
    {
        //Arrange
        var cityData = CitiesList().FirstOrDefault(x => x.CityId == Utils.DefaultId);
        var cityDtoData = CitiesDtoList().FirstOrDefault(x => x.CityId == Utils.DefaultId);
        var cityCreationDtoData = CitiesCreationDtoList().FirstOrDefault(x => x.Description == cityDtoData!.Description);

        _cityRepositoryMock.Setup(x => x.AddAsync(It.IsAny<City>(), CancellationToken.None))
                           .Callback((City city, CancellationToken _) => city.CityId = Utils.DefaultId);

        // Act
        var actual = await _cityService.CreateAsync(cityCreationDtoData!);

        // Assert
        Assert.Equal(HttpStatusCode.Created, actual.HttpCode);
        Assert.NotNull(actual.Data);
        Assert.Equal(cityDtoData?.CityId, actual.Data.CityId);
        Assert.Equal(cityDtoData?.Description, actual.Data.Description);
        _cityRepositoryMock.Verify(x => x.AddAsync(It.IsAny<City>(), CancellationToken.None), Times.Once);
    }

    #endregion CreateAsync

    #region UpdateAsync

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task UpdateAsync_ShouldUpdateCityAndReturnOk(int cityId)
    {
        // Arrange
        var cityData = CitiesList().FirstOrDefault(x => x.CityId == cityId);
        var cityUpdateDto = CitiesUpdateDtoList().FirstOrDefault(x => x.CityId == cityId);
        var cityDtoData = CitiesDtoList().FirstOrDefault(x => x.CityId == cityId);

        _cityRepositoryMock.Setup(x => x.GetByIdAsync(cityId, CancellationToken.None)).ReturnsAsync(cityData);

        cityDtoData!.Description = cityUpdateDto!.Description;
        cityData!.Description = cityUpdateDto!.Description;

        _cityRepositoryMock.Setup(x => x.UpdateAsync(cityData!, CancellationToken.None));

        // Act
        var actual = await _cityService.UpdateAsync(cityId, cityUpdateDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, actual.HttpCode);
        Assert.NotNull(actual.Data);
        Assert.Equal(cityDtoData.Description, actual.Data.Description);
        _cityRepositoryMock.Verify(x => x.GetByIdAsync(cityId, CancellationToken.None), Times.Once);
        _cityRepositoryMock.Verify(x => x.UpdateAsync(cityData!, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNotFoundForInvalidId()
    {
        // Arrange
        int invalidCityId = Utils.IdDoesNotExist;
        var cityUpdateDto = CitiesUpdateDtoList().FirstOrDefault(x => x.CityId == Utils.DefaultId);
        cityUpdateDto!.CityId = invalidCityId;

        _cityRepositoryMock.Setup(x => x.GetByIdAsync(invalidCityId, CancellationToken.None)).ReturnsAsync((City)null!);

        // Act
        var actual = await _cityService.UpdateAsync(invalidCityId, cityUpdateDto!);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, actual.HttpCode);
        Assert.Null(actual.Data);
        _cityRepositoryMock.Verify(x => x.GetByIdAsync(invalidCityId, CancellationToken.None), Times.Once);
        _cityRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<City>(), CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnBadRequestForUrlAndBodyIdMismatch()
    {
        // Arrange
        int invalidCityId = Utils.IdDoesNotExist;
        var cityUpdateDto = CitiesUpdateDtoList().FirstOrDefault(x => x.CityId == Utils.DefaultId);

        // Act
        var actual = await _cityService.UpdateAsync(invalidCityId, cityUpdateDto!);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, actual.HttpCode);
        Assert.Null(actual.Data);
        _cityRepositoryMock.Verify(x => x.GetByIdAsync(invalidCityId, CancellationToken.None), Times.Never);
        _cityRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<City>(), CancellationToken.None), Times.Never);
    }

    #endregion UpdateAsync

    #region DeleteAsync

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task CityService_DeleteAsync_ShouldDeleteCityAndReturnNoContent(int cityId)
    {
        // Arrange
        var cityToDelete = CitiesList().FirstOrDefault(x => x.CityId == cityId);

        _cityRepositoryMock.Setup(x => x.GetByIdAsync(cityId, CancellationToken.None))
            .ReturnsAsync(cityToDelete);
        _cityRepositoryMock.Setup(x => x.DeleteAsync(cityToDelete!, CancellationToken.None));

        // Act
        var actual = await _cityService.DeleteAsync(cityId);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, actual.HttpCode);
        Assert.Null(actual.Data);
        _cityRepositoryMock.Verify(x => x.GetByIdAsync(cityId, CancellationToken.None), Times.Once);
        _cityRepositoryMock.Verify(x => x.DeleteAsync(cityToDelete!, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task CityService_DeleteAsync_ShouldReturnNotFoundForInvalidId()
    {
        // Arrange
        int cityId = Utils.IdDoesNotExist;
        var cityToDelete = CitiesList().FirstOrDefault(x => x.CityId == cityId);

        _cityRepositoryMock.Setup(x => x.GetByIdAsync(cityId, CancellationToken.None)).ReturnsAsync(cityToDelete);

        // Act
        var actual = await _cityService.DeleteAsync(cityId);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, actual.HttpCode);
        Assert.Null(actual.Data);
        _cityRepositoryMock.Verify(x => x.GetByIdAsync(cityId, CancellationToken.None), Times.Once);
        _cityRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<City>(), CancellationToken.None), Times.Never);
    }

    #endregion DeleteAsync
}