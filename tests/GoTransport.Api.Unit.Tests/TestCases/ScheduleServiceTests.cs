using System.Net;
using GoTransport.Application.Dtos.Schedule;
using GoTransport.Application.Interfaces;
using GoTransport.Application.Interfaces.Base;
using GoTransport.Application.Services;
using GoTransport.Domain.Entities.App;
using Moq;

namespace GoTransport.Api.Unit.Tests.TestCases;

public class ScheduleServiceTests
{
    private readonly Mock<IRepository<Schedule>> _scheduleRepositoryMock;
    private readonly IScheduleService _scheduleService;

    public ScheduleServiceTests()
    {
        _scheduleRepositoryMock = new Mock<IRepository<Schedule>>();
        _scheduleService = new ScheduleService(_scheduleRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldStoreAPositiveDurationForAnOvernightSchedule()
    {
        // Arrange
        var creation = new ScheduleCreationDto { DepartureTime = new TimeSpan(22, 0, 0), ArrivalTime = new TimeSpan(6, 0, 0), VehicleId = 1, RouteId = 1 };

        // Act
        var actual = await _scheduleService.CreateAsync(creation);

        // Assert
        Assert.Equal(HttpStatusCode.Created, actual.HttpCode);
        Assert.Equal(TimeSpan.FromHours(8), actual.Data!.Duration);
        _scheduleRepositoryMock.Verify(x => x.AddAsync(It.Is<Schedule>(s => s.Duration == TimeSpan.FromHours(8)), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldStoreAPositiveDurationForAnOvernightSchedule()
    {
        // Arrange
        var schedule = new Schedule { ScheduleId = Guid.NewGuid(), DepartureTime = new TimeSpan(8, 0, 0), ArrivalTime = new TimeSpan(10, 0, 0), IsActive = true };
        var update = new ScheduleUpdateDto { ScheduleId = schedule.ScheduleId, DepartureTime = new TimeSpan(23, 0, 0), ArrivalTime = new TimeSpan(5, 30, 0), VehicleId = 1, RouteId = 1, IsActive = true };
        _scheduleRepositoryMock.Setup(x => x.GetByIdAsync(schedule.ScheduleId, It.IsAny<CancellationToken>())).ReturnsAsync(schedule);

        // Act
        var actual = await _scheduleService.UpdateAsync(schedule.ScheduleId, update);

        // Assert
        Assert.Equal(HttpStatusCode.OK, actual.HttpCode);
        Assert.Equal(new TimeSpan(6, 30, 0), schedule.Duration);
    }
}
