using System.Net;
using GoTransport.Api.Test.Utilities.Mothers;
using GoTransport.Application.Commons;
using GoTransport.Application.Dtos.Reservation;
using GoTransport.Application.Interfaces;
using GoTransport.Application.Interfaces.Base;
using GoTransport.Application.Services;
using GoTransport.Application.Specifications.Reservations;
using GoTransport.Application.Specifications.Vehicles;
using GoTransport.Domain.Entities.App;
using Moq;

namespace GoTransport.Api.Unit.Tests.TestCases;

public class ReservationServiceTests
{
    private readonly Mock<IRepository<Reservation>> _reservationRepositoryMock;
    private readonly Mock<IRepository<Vehicle>> _vehicleRepositoryMock;
    private readonly Mock<ICacheService<Reservation>> _cacheServiceMock;
    private readonly IReservationService _reservationService;

    public ReservationServiceTests()
    {
        _reservationRepositoryMock = new Mock<IRepository<Reservation>>();
        _vehicleRepositoryMock = new Mock<IRepository<Vehicle>>();
        _cacheServiceMock = new Mock<ICacheService<Reservation>>();
        _reservationService = new ReservationService(_reservationRepositoryMock.Object, _vehicleRepositoryMock.Object, _cacheServiceMock.Object);
    }

    #region GetAllAsync

    [Fact]
    public async Task GetAllAsync_ShouldQueryRepositoryAndPopulateCache_OnCacheMiss()
    {
        // Arrange
        var reservations = new List<Reservation> { ReservationBuilderMother.ReservationEntity(Guid.NewGuid(), DateTime.Now.Date.AddDays(1)) };
        _cacheServiceMock.Setup(x => x.Get(CacheKey.Reservations)).Returns((List<Reservation>?)null);
        _reservationRepositoryMock.Setup(x => x.ListAsync(It.IsAny<ReservationSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservations);

        // Act
        var actual = await _reservationService.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, actual.HttpCode);
        Assert.True(actual.Data?.Count() > 0);
        _reservationRepositoryMock.Verify(x => x.ListAsync(It.IsAny<ReservationSpecification>(), It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(x => x.Set(CacheKey.Reservations, It.IsAny<List<Reservation>>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnCachedData_WithoutQueryingRepository_OnCacheHit()
    {
        // Arrange
        var reservations = new List<Reservation> { ReservationBuilderMother.ReservationEntity(Guid.NewGuid(), DateTime.Now.Date.AddDays(1)) };
        _cacheServiceMock.Setup(x => x.Get(CacheKey.Reservations)).Returns(reservations);

        // Act
        var actual = await _reservationService.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, actual.HttpCode);
        _reservationRepositoryMock.Verify(x => x.ListAsync(It.IsAny<ReservationSpecification>(), It.IsAny<CancellationToken>()), Times.Never);
        _cacheServiceMock.Verify(x => x.Set(It.IsAny<string>(), It.IsAny<List<Reservation>>()), Times.Never);
    }

    #endregion GetAllAsync

    #region GetByScheduleAsync

    [Fact]
    public async Task GetByScheduleAsync_ShouldReturnPagedReservations()
    {
        // Arrange
        var scheduleId = Guid.NewGuid();
        var reservationDtoData = new List<ReservationDto> { new() { ReservationId = Guid.NewGuid(), PassengerFirstName = "John" } };
        _reservationRepositoryMock.Setup(x => x.ListAsync(It.IsAny<PagedReservationSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservationDtoData);
        _reservationRepositoryMock.Setup(x => x.CountAsync(It.IsAny<ReservationSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservationDtoData.Count);

        // Act
        var actual = await _reservationService.GetByScheduleAsync(scheduleId, ReservationBuilderMother.ReservationParameters(), CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, actual.HttpCode);
        Assert.True(actual.Data?.Count() > 0);
        _reservationRepositoryMock.Verify(x => x.ListAsync(It.IsAny<PagedReservationSpecification>(), It.IsAny<CancellationToken>()), Times.Once);
        _reservationRepositoryMock.Verify(x => x.CountAsync(It.IsAny<ReservationSpecification>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion GetByScheduleAsync

    #region GetDetailsByPerson

    [Fact]
    public async Task GetDetailsByPerson_ShouldReturnReservationWhenFound()
    {
        // Arrange
        var reservation = ReservationBuilderMother.ReservationEntity(Guid.NewGuid(), DateTime.Now.Date.AddDays(1));
        _reservationRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<ReservationSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        // Act
        var actual = await _reservationService.GetDetailsByPerson(reservation.ReservationId.ToString(), reservation.PassengerIdentification, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, actual.HttpCode);
        Assert.NotNull(actual.Data);
        Assert.Equal(reservation.PassengerIdentification, actual.Data!.PassengerIdentification);
    }

    [Fact]
    public async Task GetDetailsByPerson_ShouldReturnNotFoundWhenMissing()
    {
        // Arrange
        _reservationRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<ReservationSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Reservation)null!);

        // Act
        var actual = await _reservationService.GetDetailsByPerson(Guid.NewGuid().ToString(), "000", CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, actual.HttpCode);
        Assert.Null(actual.Data);
    }

    #endregion GetDetailsByPerson

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ShouldReturnReservationForValidId()
    {
        // Arrange
        var reservation = ReservationBuilderMother.ReservationEntity(Guid.NewGuid(), DateTime.Now.Date.AddDays(1));
        _reservationRepositoryMock.Setup(x => x.GetByIdAsync(reservation.ReservationId, It.IsAny<CancellationToken>())).ReturnsAsync(reservation);

        // Act
        var actual = await _reservationService.GetByIdAsync(reservation.ReservationId, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, actual.HttpCode);
        Assert.NotNull(actual.Data);
        Assert.Equal(reservation.ReservationId, actual.Data!.ReservationId);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNotFoundForInvalidId()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        _reservationRepositoryMock.Setup(x => x.GetByIdAsync(reservationId, It.IsAny<CancellationToken>())).ReturnsAsync((Reservation)null!);

        // Act
        var actual = await _reservationService.GetByIdAsync(reservationId, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, actual.HttpCode);
        Assert.Null(actual.Data);
    }

    #endregion GetByIdAsync

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_ShouldCreateReservationWhenSeatsAvailable()
    {
        // Arrange
        var scheduleId = Guid.NewGuid();
        var creationDto = ReservationBuilderMother.ReservationCreationDtoOk(scheduleId);

        _vehicleRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<VehicleSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Vehicle { VehicleId = 1, Capacity = 10 });
        _reservationRepositoryMock.Setup(x => x.CountAsync(It.IsAny<ReservationSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var actual = await _reservationService.CreateAsync(creationDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, actual.HttpCode);
        Assert.NotNull(actual.Data);
        _reservationRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnConflictWhenVehicleIsFull()
    {
        // Arrange
        var scheduleId = Guid.NewGuid();
        var creationDto = ReservationBuilderMother.ReservationCreationDtoOk(scheduleId);

        _vehicleRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<VehicleSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Vehicle { VehicleId = 1, Capacity = 5 });
        _reservationRepositoryMock.Setup(x => x.CountAsync(It.IsAny<ReservationSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        // Act
        var actual = await _reservationService.CreateAsync(creationDto);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, actual.HttpCode);
        Assert.Null(actual.Data);
        Assert.Contains(ErrorMessages.VehicleFull, actual.Errors!);
        _reservationRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion CreateAsync

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_ShouldUpdateReservationAndReturnOk()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        var scheduleId = Guid.NewGuid();
        var reservation = ReservationBuilderMother.ReservationEntity(reservationId, DateTime.Now.Date.AddDays(1));
        var updateDto = ReservationBuilderMother.ReservationUpdateDtoOk(reservationId, scheduleId);

        _reservationRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<ReservationSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        // Act
        var actual = await _reservationService.UpdateAsync(reservationId, updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, actual.HttpCode);
        Assert.NotNull(actual.Data);
        _reservationRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnBadRequestForUrlAndBodyIdMismatch()
    {
        // Arrange
        var updateDto = ReservationBuilderMother.ReservationUpdateDtoOk(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var actual = await _reservationService.UpdateAsync(Guid.NewGuid(), updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, actual.HttpCode);
        Assert.Null(actual.Data);
        Assert.Contains(ErrorMessages.UrlAndBodyIdNotEqual, actual.Errors!);
        _reservationRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNotFoundForInvalidId()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        var updateDto = ReservationBuilderMother.ReservationUpdateDtoOk(reservationId, Guid.NewGuid());
        _reservationRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<ReservationSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Reservation)null!);

        // Act
        var actual = await _reservationService.UpdateAsync(reservationId, updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, actual.HttpCode);
        Assert.Null(actual.Data);
        _reservationRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnBadRequestWhenReservationDatePassed()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        var updateDto = ReservationBuilderMother.ReservationUpdateDtoOk(reservationId, Guid.NewGuid());
        var pastReservation = ReservationBuilderMother.ReservationEntity(reservationId, DateTime.Now.Date.AddDays(-1));
        _reservationRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<ReservationSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pastReservation);

        // Act
        var actual = await _reservationService.UpdateAsync(reservationId, updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, actual.HttpCode);
        Assert.Null(actual.Data);
        Assert.Contains(ErrorMessages.ReservationDatePassed, actual.Errors!);
        _reservationRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion UpdateAsync

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_ShouldDeleteReservationAndReturnNoContent()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        var reservation = ReservationBuilderMother.ReservationEntity(reservationId, DateTime.Now.Date.AddDays(1));
        _reservationRepositoryMock.Setup(x => x.GetByIdAsync(reservationId, It.IsAny<CancellationToken>())).ReturnsAsync(reservation);

        // Act
        var actual = await _reservationService.DeleteAsync(reservationId);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, actual.HttpCode);
        Assert.Null(actual.Data);
        _reservationRepositoryMock.Verify(x => x.DeleteAsync(reservation, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnNotFoundForInvalidId()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        _reservationRepositoryMock.Setup(x => x.GetByIdAsync(reservationId, It.IsAny<CancellationToken>())).ReturnsAsync((Reservation)null!);

        // Act
        var actual = await _reservationService.DeleteAsync(reservationId);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, actual.HttpCode);
        Assert.Null(actual.Data);
        _reservationRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnBadRequestWhenReservationDatePassed()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        var pastReservation = ReservationBuilderMother.ReservationEntity(reservationId, DateTime.Now.Date.AddDays(-1));
        _reservationRepositoryMock.Setup(x => x.GetByIdAsync(reservationId, It.IsAny<CancellationToken>())).ReturnsAsync(pastReservation);

        // Act
        var actual = await _reservationService.DeleteAsync(reservationId);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, actual.HttpCode);
        Assert.Null(actual.Data);
        Assert.Contains(ErrorMessages.ReservationDatePassed, actual.Errors!);
        _reservationRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion DeleteAsync
}
