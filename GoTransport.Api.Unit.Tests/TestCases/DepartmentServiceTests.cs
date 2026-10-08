using System.Net;
using GoTransport.Api.Test.Utilities.Commons;
using GoTransport.Api.Test.Utilities.Mothers;
using GoTransport.Application.Commons;
using GoTransport.Application.Dtos.Department;
using GoTransport.Application.Interfaces;
using GoTransport.Application.Interfaces.Base;
using GoTransport.Application.Services;
using GoTransport.Application.Specifications.Departments;
using GoTransport.Domain.Entities.Bas;
using Moq;

namespace GoTransport.Api.Unit.Tests.TestCases;

public class DepartmentServiceTests
{
    private readonly Mock<IRepository<Department>> _departmentRepositoryMock;
    private readonly Mock<ICacheService<Department>> _cacheServiceMock;
    private readonly IDepartmentService _departmentService;

    public DepartmentServiceTests()
    {
        _departmentRepositoryMock = new Mock<IRepository<Department>>();
        _cacheServiceMock = new Mock<ICacheService<Department>>();
        _departmentService = new DepartmentService(_departmentRepositoryMock.Object, _cacheServiceMock.Object);
    }

    #region DataSeed

    private static List<Department> DepartmentsList()
    {
        return new List<Department>
        {
            new() { DepartmentId = 1, Description = "Antioquia", IsActive = true },
            new() { DepartmentId = 2, Description = "Cundinamarca", IsActive = true },
            new() { DepartmentId = 3, Description = "Valle del Cauca", IsActive = true }
        };
    }

    private static List<DepartmentDto> DepartmentsDtoList()
    {
        return new List<DepartmentDto>
        {
            new() { DepartmentId = 1, Description = "Antioquia", IsActive = true },
            new() { DepartmentId = 2, Description = "Cundinamarca", IsActive = true },
            new() { DepartmentId = 3, Description = "Valle del Cauca", IsActive = true }
        };
    }

    #endregion DataSeed

    #region GetAllAsync

    [Fact]
    public async Task GetAllAsync_ShouldQueryRepositoryAndPopulateCache_OnCacheMiss()
    {
        // Arrange
        _cacheServiceMock.Setup(x => x.Get(CacheKey.Departments)).Returns((List<Department>?)null);
        _departmentRepositoryMock.Setup(x => x.ListAsync(It.IsAny<DepartmentSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DepartmentsList());

        // Act
        var actual = await _departmentService.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, actual.HttpCode);
        Assert.True(actual.Data?.Count() > 0);
        _departmentRepositoryMock.Verify(x => x.ListAsync(It.IsAny<DepartmentSpecification>(), It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(x => x.Set(CacheKey.Departments, It.IsAny<List<Department>>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnCachedData_WithoutQueryingRepository_OnCacheHit()
    {
        // Arrange
        _cacheServiceMock.Setup(x => x.Get(CacheKey.Departments)).Returns(DepartmentsList());

        // Act
        var actual = await _departmentService.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, actual.HttpCode);
        Assert.True(actual.Data?.Count() > 0);
        _departmentRepositoryMock.Verify(x => x.ListAsync(It.IsAny<DepartmentSpecification>(), It.IsAny<CancellationToken>()), Times.Never);
        _cacheServiceMock.Verify(x => x.Set(It.IsAny<string>(), It.IsAny<List<Department>>()), Times.Never);
    }

    #endregion GetAllAsync

    #region GetAsync

    [Fact]
    public async Task GetAsync_ShouldReturnDepartmentsForValidParameters()
    {
        // Arrange
        var departmentDtoData = DepartmentsDtoList();
        _departmentRepositoryMock.Setup(x => x.ListAsync(It.IsAny<PagedDepartmentSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(departmentDtoData);
        _departmentRepositoryMock.Setup(x => x.CountAsync(It.IsAny<DepartmentSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(departmentDtoData.Count);

        // Act
        var actual = await _departmentService.GetAsync(DepartmentBuilderMother.DepartmentParameters(), CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, actual.HttpCode);
        Assert.True(actual.Data?.Count() > 0);
        _departmentRepositoryMock.Verify(x => x.ListAsync(It.IsAny<PagedDepartmentSpecification>(), It.IsAny<CancellationToken>()), Times.Once);
        _departmentRepositoryMock.Verify(x => x.CountAsync(It.IsAny<DepartmentSpecification>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnEmptyListWhenNoMatches()
    {
        // Arrange
        var departmentDtoData = new List<DepartmentDto>();
        _departmentRepositoryMock.Setup(x => x.ListAsync(It.IsAny<PagedDepartmentSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(departmentDtoData);
        _departmentRepositoryMock.Setup(x => x.CountAsync(It.IsAny<DepartmentSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(departmentDtoData.Count);

        var parameters = DepartmentBuilderMother.DepartmentParameters(search: "non-existent");

        // Act
        var actual = await _departmentService.GetAsync(parameters, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, actual.HttpCode);
        Assert.True(actual.Data?.Count() == 0);
        _departmentRepositoryMock.Verify(x => x.ListAsync(It.IsAny<PagedDepartmentSpecification>(), It.IsAny<CancellationToken>()), Times.Once);
        _departmentRepositoryMock.Verify(x => x.CountAsync(It.IsAny<DepartmentSpecification>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion GetAsync

    #region GetByIdAsync

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task GetByIdAsync_ShouldReturnDepartmentForValidId(int departmentId)
    {
        // Arrange
        var departmentData = DepartmentsList().FirstOrDefault(x => x.DepartmentId == departmentId);
        _departmentRepositoryMock.Setup(x => x.GetByIdAsync(departmentId, It.IsAny<CancellationToken>())).ReturnsAsync(departmentData);

        // Act
        var actual = await _departmentService.GetByIdAsync(departmentId, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, actual.HttpCode);
        Assert.Equal(departmentData?.Description, actual.Data!.Description);
        _departmentRepositoryMock.Verify(x => x.GetByIdAsync(departmentId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNotFoundForInvalidId()
    {
        // Arrange
        int departmentId = Utils.IdDoesNotExist;
        _departmentRepositoryMock.Setup(x => x.GetByIdAsync(departmentId, It.IsAny<CancellationToken>())).ReturnsAsync((Department)null!);

        // Act
        var actual = await _departmentService.GetByIdAsync(departmentId, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, actual.HttpCode);
        Assert.Null(actual.Data);
        _departmentRepositoryMock.Verify(x => x.GetByIdAsync(departmentId, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion GetByIdAsync

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_ShouldCreateDepartmentAndReturnCreated()
    {
        // Arrange
        var creationDto = DepartmentBuilderMother.DepartmentCreationDtoOk();
        _departmentRepositoryMock.Setup(x => x.AnyAsync(It.IsAny<DuplicationDepartmentSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _departmentRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Department>(), It.IsAny<CancellationToken>()))
            .Callback((Department department, CancellationToken _) => department.DepartmentId = Utils.DefaultId);

        // Act
        var actual = await _departmentService.CreateAsync(creationDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, actual.HttpCode);
        Assert.NotNull(actual.Data);
        Assert.Equal(creationDto.Description, actual.Data!.Description);
        _departmentRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Department>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnBadRequestForDuplicateDescription()
    {
        // Arrange
        var creationDto = DepartmentBuilderMother.DepartmentCreationDtoOk();
        _departmentRepositoryMock.Setup(x => x.AnyAsync(It.IsAny<DuplicationDepartmentSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var actual = await _departmentService.CreateAsync(creationDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, actual.HttpCode);
        Assert.Null(actual.Data);
        Assert.Contains(ErrorMessages.DuplicateDescription, actual.Errors!);
        _departmentRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Department>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion CreateAsync

    #region UpdateAsync

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task UpdateAsync_ShouldUpdateDepartmentAndReturnOk(int departmentId)
    {
        // Arrange
        var departmentData = DepartmentsList().FirstOrDefault(x => x.DepartmentId == departmentId);
        var updateDto = DepartmentBuilderMother.DepartmentUpdateDtoOk(departmentId);

        _departmentRepositoryMock.Setup(x => x.GetByIdAsync(departmentId, It.IsAny<CancellationToken>())).ReturnsAsync(departmentData);
        _departmentRepositoryMock.Setup(x => x.AnyAsync(It.IsAny<DuplicationDepartmentSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var actual = await _departmentService.UpdateAsync(departmentId, updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, actual.HttpCode);
        Assert.NotNull(actual.Data);
        Assert.Equal(updateDto.Description, actual.Data!.Description);
        _departmentRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Department>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnBadRequestForUrlAndBodyIdMismatch()
    {
        // Arrange
        var updateDto = DepartmentBuilderMother.DepartmentUpdateDtoOk(Utils.DefaultId);

        // Act
        var actual = await _departmentService.UpdateAsync(Utils.IdDoesNotExist, updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, actual.HttpCode);
        Assert.Null(actual.Data);
        _departmentRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _departmentRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Department>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNotFoundForInvalidId()
    {
        // Arrange
        int departmentId = Utils.IdDoesNotExist;
        var updateDto = DepartmentBuilderMother.DepartmentUpdateDtoOk(departmentId);
        _departmentRepositoryMock.Setup(x => x.GetByIdAsync(departmentId, It.IsAny<CancellationToken>())).ReturnsAsync((Department)null!);

        // Act
        var actual = await _departmentService.UpdateAsync(departmentId, updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, actual.HttpCode);
        Assert.Null(actual.Data);
        _departmentRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Department>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnBadRequestForDuplicateDescription()
    {
        // Arrange
        int departmentId = Utils.DefaultId;
        var departmentData = DepartmentsList().FirstOrDefault(x => x.DepartmentId == departmentId);
        var updateDto = DepartmentBuilderMother.DepartmentUpdateDtoOk(departmentId);

        _departmentRepositoryMock.Setup(x => x.GetByIdAsync(departmentId, It.IsAny<CancellationToken>())).ReturnsAsync(departmentData);
        _departmentRepositoryMock.Setup(x => x.AnyAsync(It.IsAny<DuplicationDepartmentSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var actual = await _departmentService.UpdateAsync(departmentId, updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, actual.HttpCode);
        Assert.Null(actual.Data);
        Assert.Contains(ErrorMessages.DuplicateDescription, actual.Errors!);
        _departmentRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Department>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion UpdateAsync

    #region DeleteAsync

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task DeleteAsync_ShouldDeleteDepartmentAndReturnNoContent(int departmentId)
    {
        // Arrange
        var departmentData = DepartmentsList().FirstOrDefault(x => x.DepartmentId == departmentId);
        _departmentRepositoryMock.Setup(x => x.GetByIdAsync(departmentId, It.IsAny<CancellationToken>())).ReturnsAsync(departmentData);

        // Act
        var actual = await _departmentService.DeleteAsync(departmentId);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, actual.HttpCode);
        Assert.Null(actual.Data);
        _departmentRepositoryMock.Verify(x => x.DeleteAsync(departmentData!, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnNotFoundForInvalidId()
    {
        // Arrange
        int departmentId = Utils.IdDoesNotExist;
        _departmentRepositoryMock.Setup(x => x.GetByIdAsync(departmentId, It.IsAny<CancellationToken>())).ReturnsAsync((Department)null!);

        // Act
        var actual = await _departmentService.DeleteAsync(departmentId);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, actual.HttpCode);
        Assert.Null(actual.Data);
        _departmentRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Department>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion DeleteAsync
}
