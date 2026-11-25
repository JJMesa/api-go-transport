using GoTransport.Application.Extensions;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Api.Unit.Tests.TestCases;

public class ScheduleExtensionsTests
{
    [Theory]
    [InlineData("06:00", "15:00", "09:00")]
    [InlineData("22:00", "06:00", "08:00")]
    [InlineData("23:30", "00:00", "00:30")]
    [InlineData("08:00", "08:00", "1.00:00")]
    public void TripDuration_ShouldCountAnEarlierArrivalAsNextDay(string departure, string arrival, string expected)
    {
        // Act
        var actual = ScheduleExtensions.TripDuration(TimeSpan.Parse(departure), TimeSpan.Parse(arrival));

        // Assert
        Assert.Equal(TimeSpan.Parse(expected), actual);
    }

    [Fact]
    public void ArrivalOn_ShouldFallOnTheNextDayForAnOvernightSchedule()
    {
        // Arrange
        var schedule = new Schedule { DepartureTime = new TimeSpan(22, 0, 0), ArrivalTime = new TimeSpan(6, 0, 0) };

        // Act
        var actual = schedule.ArrivalOn(new DateTime(2026, 12, 24, 15, 30, 0));

        // Assert
        Assert.Equal(new DateTime(2026, 12, 25, 6, 0, 0), actual);
    }

    [Fact]
    public void ArrivalOn_ShouldStayOnTheTravelDateForADaySchedule()
    {
        // Arrange
        var schedule = new Schedule { DepartureTime = new TimeSpan(6, 0, 0), ArrivalTime = new TimeSpan(15, 0, 0) };

        // Act
        var actual = schedule.ArrivalOn(new DateTime(2026, 12, 24));

        // Assert
        Assert.Equal(new DateTime(2026, 12, 24, 15, 0, 0), actual);
    }
}
