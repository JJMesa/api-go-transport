using System.Data;
using System.Net;
using GoTransport.Api.Unit.Tests.Helpers;
using GoTransport.Api.Test.Utilities.Mothers;
using GoTransport.Application.Commons;
using GoTransport.Application.Dtos.Reservation;
using GoTransport.Application.Interfaces;
using GoTransport.Application.Interfaces.Base;
using GoTransport.Application.Services;
using GoTransport.Application.Specifications.Reservations;
using GoTransport.Application.Specifications.Schedules;
using GoTransport.Domain.Entities.App;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace GoTransport.Api.Unit.Tests.TestCases;

public class ReservationServiceTests
{
    private readonly Mock<IRepository<Reservation>> _reservationRepositoryMock;
    private readonly Mock<IRepository<Schedule>> _scheduleRepositoryMock;
    private readonly InlineUnitOfWork _unitOfWork;
    private readonly FakeTimeProvider _clock;
    private readonly IReservationService _reservationService;

    public ReservationServiceTests()
    {
        _reservationRepositoryMock = new Mock<IRepository<Reservation>>();
        _scheduleRepositoryMock = new Mock<IRepository<Schedule>>();
        _unitOfWork = new InlineUnitOfWork();
        _clock = new FakeTimeProvider(DateTimeOffset.UtcNow);
        _reservationService = new ReservationService(_reservationRepositoryMock.Object, _scheduleRepositoryMock.Object, _unitOfWork, _clock);
    }

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

    #region GetByPassengerAsync

    [Fact]
    public async Task GetByPassengerAsync_ShouldReturnReservationWhenFound()
    {
        // Arrange
        var reservation = ReservationBuilderMother.ReservationEntity(Guid.NewGuid(), DateTime.Now.Date.AddDays(1));
        _reservationRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<ReservationSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        // Act
        var actual = await _reservationService.GetByPassengerAsync(reservation.ReservationId, reservation.PassengerIdentification, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, actual.HttpCode);
        Assert.NotNull(actual.Data);
        Assert.Equal(reservation.PassengerIdentification, actual.Data!.PassengerIdentification);
    }

    [Fact]
    public async Task GetByPassengerAsync_ShouldReturnNotFoundWhenMissing()
    {
        // Arrange
        _reservationRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<ReservationSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Reservation)null!);

        // Act
        var actual = await _reservationService.GetByPassengerAsync(Guid.NewGuid(), "000", CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, actual.HttpCode);
        Assert.Null(actual.Data);
    }

    #endregion GetByPassengerAsync

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ShouldReturnReservationForValidId()
    {
        // Arrange
        var reservation = ReservationBuilderMother.ReservationEntity(Guid.NewGuid(), DateTime.Now.Date.AddDays(1));
        _reservationRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<ReservationDetailSpecification>(), It.IsAny<CancellationToken>())).ReturnsAsync(reservation);

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

        _scheduleRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<ScheduleSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ReservationBuilderMother.ScheduleEntity(scheduleId, capacity: 10));
        _reservationRepositoryMock.Setup(x => x.CountAsync(It.IsAny<TripReservationSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var actual = await _reservationService.CreateAsync(creationDto, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.Created, actual.HttpCode);
        Assert.NotNull(actual.Data);
        _reservationRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnConflictWhenVehicleIsFull()
    {
        // Arrange
        var scheduleId = Guid.NewGuid();
        var creationDto = ReservationBuilderMother.ReservationCreationDtoOk(scheduleId);

        _scheduleRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<ScheduleSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ReservationBuilderMother.ScheduleEntity(scheduleId, capacity: 5));
        _reservationRepositoryMock.Setup(x => x.CountAsync(It.IsAny<TripReservationSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        // Act
        var actual = await _reservationService.CreateAsync(creationDto, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, actual.HttpCode);
        Assert.Null(actual.Data);
        Assert.Contains(ErrorMessages.VehicleFull, actual.Errors!);
        _reservationRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldCountAndInsertInOneSerializableTransaction()
    {
        // Arrange
        var creationDto = ReservationBuilderMother.ReservationCreationDtoOk(Guid.NewGuid());
        _scheduleRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<ScheduleSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ReservationBuilderMother.ScheduleEntity(creationDto.ScheduleId));

        // Act
        await _reservationService.CreateAsync(creationDto, CancellationToken.None);

        // Assert
        Assert.Equal(new[] { IsolationLevel.Serializable }, _unitOfWork.Transactions);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnBadRequestForATripThatHasEnded()
    {
        // Arrange
        var creationDto = ReservationBuilderMother.ReservationCreationDtoOk(Guid.NewGuid());
        creationDto.ReservationDate = DateTime.Now.Date.AddDays(-1);
        _scheduleRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<ScheduleSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ReservationBuilderMother.ScheduleEntity(creationDto.ScheduleId));

        // Act
        var actual = await _reservationService.CreateAsync(creationDto, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, actual.HttpCode);
        Assert.Contains(ErrorMessages.TravelDatePassed, actual.Errors);
        _reservationRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnNotFoundForUnknownSchedule()
    {
        // Arrange
        var creationDto = ReservationBuilderMother.ReservationCreationDtoOk(Guid.NewGuid());
        _scheduleRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<ScheduleSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Schedule)null!);

        // Act
        var actual = await _reservationService.CreateAsync(creationDto, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, actual.HttpCode);
        Assert.Contains(ErrorMessages.InvalidSchedule, actual.Errors);
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
        _scheduleRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<ScheduleSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ReservationBuilderMother.ScheduleEntity(scheduleId));

        // Act
        var actual = await _reservationService.UpdateAsync(reservationId, updateDto, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, actual.HttpCode);
        Assert.NotNull(actual.Data);
        _reservationRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(new[] { IsolationLevel.Serializable }, _unitOfWork.Transactions);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnConflictWhenTheTargetTripIsFull()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        var targetScheduleId = Guid.NewGuid();
        var reservation = ReservationBuilderMother.ReservationEntity(reservationId, DateTime.Now.Date.AddDays(1));
        var updateDto = ReservationBuilderMother.ReservationUpdateDtoOk(reservationId, targetScheduleId);
        _reservationRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<ReservationSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);
        _scheduleRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<ScheduleSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ReservationBuilderMother.ScheduleEntity(targetScheduleId, capacity: 3));
        _reservationRepositoryMock.Setup(x => x.CountAsync(It.IsAny<TripReservationSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        // Act
        var actual = await _reservationService.UpdateAsync(reservationId, updateDto, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, actual.HttpCode);
        Assert.Contains(ErrorMessages.VehicleFull, actual.Errors);
        _reservationRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnBadRequestWhenTheTargetTripHasEnded()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        var reservation = ReservationBuilderMother.ReservationEntity(reservationId, DateTime.Now.Date.AddDays(1));
        var updateDto = ReservationBuilderMother.ReservationUpdateDtoOk(reservationId, reservation.ScheduleId);
        updateDto.ReservationDate = DateTime.Now.Date.AddDays(-3);
        _reservationRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<ReservationSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);
        _scheduleRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<ScheduleSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation.Schedule);

        // Act
        var actual = await _reservationService.UpdateAsync(reservationId, updateDto, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, actual.HttpCode);
        Assert.Contains(ErrorMessages.TravelDatePassed, actual.Errors);
        _reservationRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNotFoundForUnknownTargetSchedule()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        var reservation = ReservationBuilderMother.ReservationEntity(reservationId, DateTime.Now.Date.AddDays(1));
        var updateDto = ReservationBuilderMother.ReservationUpdateDtoOk(reservationId, Guid.NewGuid());
        _reservationRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<ReservationSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);
        _scheduleRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<ScheduleSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Schedule)null!);

        // Act
        var actual = await _reservationService.UpdateAsync(reservationId, updateDto, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, actual.HttpCode);
        Assert.Contains(ErrorMessages.InvalidSchedule, actual.Errors);
    }

    [Fact]
    public async Task UpdateAsync_ShouldNotRecountCapacityWhenStayingOnTheSameTrip()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        var reservation = ReservationBuilderMother.ReservationEntity(reservationId, DateTime.Now.Date.AddDays(1));
        var updateDto = ReservationBuilderMother.ReservationUpdateDtoOk(reservationId, reservation.ScheduleId);
        _reservationRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<ReservationSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        // Act
        var actual = await _reservationService.UpdateAsync(reservationId, updateDto, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, actual.HttpCode);
        _reservationRepositoryMock.Verify(x => x.CountAsync(It.IsAny<TripReservationSpecification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnBadRequestForUrlAndBodyIdMismatch()
    {
        // Arrange
        var updateDto = ReservationBuilderMother.ReservationUpdateDtoOk(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var actual = await _reservationService.UpdateAsync(Guid.NewGuid(), updateDto, CancellationToken.None);

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
        var actual = await _reservationService.UpdateAsync(reservationId, updateDto, CancellationToken.None);

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
        var actual = await _reservationService.UpdateAsync(reservationId, updateDto, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, actual.HttpCode);
        Assert.Null(actual.Data);
        Assert.Contains(ErrorMessages.ReservationDatePassed, actual.Errors!);
        _reservationRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion UpdateAsync

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_ShouldCancelTheReservationInsteadOfDeletingIt()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        var reservation = ReservationBuilderMother.ReservationEntity(reservationId, DateTime.Now.Date.AddDays(1));
        _reservationRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<ReservationSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        // Act
        var actual = await _reservationService.DeleteAsync(reservationId, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, actual.HttpCode);
        Assert.Null(actual.Data);
        Assert.False(reservation.IsActive);
        _reservationRepositoryMock.Verify(x => x.UpdateAsync(reservation, It.IsAny<CancellationToken>()), Times.Once);
        _reservationRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnNotFoundForACancelledReservation()
    {
        // Arrange
        var reservation = ReservationBuilderMother.ReservationEntity(Guid.NewGuid(), DateTime.Now.Date.AddDays(1));
        reservation.IsActive = false;
        _reservationRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<ReservationSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        // Act
        var actual = await _reservationService.DeleteAsync(reservation.ReservationId, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, actual.HttpCode);
        _reservationRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNotFoundForACancelledReservation()
    {
        // Arrange
        var reservation = ReservationBuilderMother.ReservationEntity(Guid.NewGuid(), DateTime.Now.Date.AddDays(1));
        reservation.IsActive = false;
        _reservationRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<ReservationSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        // Act
        var actual = await _reservationService.UpdateAsync(reservation.ReservationId,
            ReservationBuilderMother.ReservationUpdateDtoOk(reservation.ReservationId, reservation.ScheduleId), CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, actual.HttpCode);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnNotFoundForInvalidId()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        _reservationRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<ReservationSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Reservation)null!);

        // Act
        var actual = await _reservationService.DeleteAsync(reservationId, CancellationToken.None);

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
        _reservationRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<ReservationSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pastReservation);

        // Act
        var actual = await _reservationService.DeleteAsync(reservationId, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, actual.HttpCode);
        Assert.Null(actual.Data);
        Assert.Contains(ErrorMessages.ReservationDatePassed, actual.Errors!);
        _reservationRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("14:59", HttpStatusCode.NoContent)]
    [InlineData("15:01", HttpStatusCode.BadRequest)]
    public async Task DeleteAsync_ShouldJudgeTheTripEndInColombianTime(string colombianNow, HttpStatusCode expected)
    {
        // Arrange: a Trip that arrives at 15:00 Colombian time on 2026-12-24 (UTC−5, so 20:00 UTC)
        var schedule = ReservationBuilderMother.ScheduleEntity(arrivalTime: new TimeSpan(15, 0, 0));
        var reservation = ReservationBuilderMother.ReservationEntity(Guid.NewGuid(), new DateTime(2026, 12, 24), schedule);
        _reservationRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<ReservationSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);
        var now = TimeSpan.Parse(colombianNow);
        _clock.SetUtcNow(new DateTimeOffset(2026, 12, 24, now.Hours, now.Minutes, 0, TimeSpan.FromHours(-5)));

        // Act
        var actual = await _reservationService.DeleteAsync(reservation.ReservationId, CancellationToken.None);

        // Assert
        Assert.Equal(expected, actual.HttpCode);
    }

    #endregion DeleteAsync
}
