using GoTransport.Application.Specifications.Reservations;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Api.Unit.Tests.TestCases;

public class TripReservationSpecificationTests
{
    private static readonly Guid ScheduleId = Guid.NewGuid();
    private static readonly DateTime TravelDate = new(2026, 12, 24);

    [Fact]
    public void Evaluate_ShouldKeepOnlyActiveReservationsOnTheSameScheduleAndDate()
    {
        // Arrange
        var onTrip = Reservation(ScheduleId, TravelDate);
        var reservations = new List<Reservation>
        {
            onTrip,
            Reservation(ScheduleId, TravelDate.AddDays(1)),
            Reservation(Guid.NewGuid(), TravelDate),
            Reservation(ScheduleId, TravelDate, isActive: false)
        };

        // Act
        var actual = new TripReservationSpecification(ScheduleId, TravelDate.AddHours(10)).Evaluate(reservations);

        // Assert
        Assert.Equal(new[] { onTrip }, actual);
    }

    [Fact]
    public void Evaluate_ShouldLeaveOutTheExcludedReservation()
    {
        // Arrange
        var moving = Reservation(ScheduleId, TravelDate);
        var other = Reservation(ScheduleId, TravelDate);

        // Act
        var actual = new TripReservationSpecification(ScheduleId, TravelDate, moving.ReservationId).Evaluate(new[] { moving, other });

        // Assert
        Assert.Equal(new[] { other }, actual);
    }

    private static Reservation Reservation(Guid scheduleId, DateTime travelDate, bool isActive = true) => new()
    {
        ReservationId = Guid.NewGuid(),
        ScheduleId = scheduleId,
        ReservationDate = travelDate,
        IsActive = isActive
    };
}
