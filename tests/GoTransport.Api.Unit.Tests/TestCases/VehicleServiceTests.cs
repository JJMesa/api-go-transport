using System.Net;
using GoTransport.Application.Commons;
using GoTransport.Application.Dtos.Vehicle;
using GoTransport.Application.Interfaces;
using GoTransport.Application.Services;
using GoTransport.Application.Specifications.Vehicles;
using GoTransport.Domain.Entities.App;
using GoTransport.Domain.Interfaces;
using Moq;

namespace GoTransport.Api.Unit.Tests.TestCases;

public class VehicleServiceTests
{
    private readonly Mock<IRepository<Vehicle>> _vehicleRepositoryMock = new();
    private readonly Mock<ICacheService<Vehicle>> _cacheServiceMock = new();
    private readonly IVehicleService _vehicleService;

    public VehicleServiceTests()
    {
        _vehicleService = new VehicleService(_vehicleRepositoryMock.Object, _cacheServiceMock.Object);
    }

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_ShouldAddTheVehicleAndDropTheCachedList()
    {
        // Act
        var actual = await _vehicleService.CreateAsync(Creation(), CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.Created, actual.HttpCode);
        Assert.Equal("XYZ987", actual.Data!.LicensePlate);
        _vehicleRepositoryMock.Verify(x => x.AddAsync(It.Is<Vehicle>(v => v.LicensePlate == "XYZ987" && v.Capacity == 40), It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(x => x.Remove(CacheKey.Vehicles), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnConflictForADuplicateLicensePlate()
    {
        // Arrange
        _vehicleRepositoryMock.Setup(x => x.AnyAsync(It.IsAny<DuplicationVehicleSpecification>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var actual = await _vehicleService.CreateAsync(Creation(), CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, actual.HttpCode);
        Assert.Contains(ErrorMessages.DuplicatePlate, actual.Errors!);
        _vehicleRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()), Times.Never);
        _cacheServiceMock.Verify(x => x.Remove(It.IsAny<string>()), Times.Never);
    }

    #endregion CreateAsync

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_ShouldChangeTheCapacityAndDropTheCachedList()
    {
        // Arrange
        var vehicle = new Vehicle { VehicleId = 1, LicensePlate = "ABC123", Model = 2022, Capacity = 40, IsActive = true };
        _vehicleRepositoryMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(vehicle);

        // Act
        var actual = await _vehicleService.UpdateAsync(1, new VehicleUpdateDto { VehicleId = 1, Capacity = 32 }, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, actual.HttpCode);
        Assert.Equal(32, vehicle.Capacity);
        _vehicleRepositoryMock.Verify(x => x.UpdateAsync(vehicle, It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(x => x.Remove(CacheKey.Vehicles), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNotFoundForUnknownVehicle()
    {
        // Act
        var actual = await _vehicleService.UpdateAsync(1, new VehicleUpdateDto { VehicleId = 1, Capacity = 32 }, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, actual.HttpCode);
        _vehicleRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnBadRequestWhenUrlAndBodyIdsDiffer()
    {
        // Act
        var actual = await _vehicleService.UpdateAsync(2, new VehicleUpdateDto { VehicleId = 1, Capacity = 32 }, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, actual.HttpCode);
    }

    #endregion UpdateAsync

    private static VehicleCreationDto Creation() => new()
    {
        ManufacturerId = 1,
        LicensePlate = "XYZ987",
        Model = 2024,
        Capacity = 40
    };
}
