using Ardalis.Specification;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Specifications.Reservations;

/// <summary>
/// The active Reservations on one Trip: a Schedule on a travel date. Counting them gives the places
/// taken on that Trip, which is what the Vehicle's Capacity limits.
/// </summary>
public class TripReservationSpecification : Specification<Reservation>
{
    public TripReservationSpecification(Guid scheduleId, DateTime travelDate, Guid? excludedReservationId = null)
    {
        var date = travelDate.Date;

        Query.Where(reservation =>
            reservation.ScheduleId == scheduleId
            && reservation.ReservationDate == date
            && reservation.IsActive == true);

        // A Reservation being moved must not count against the place it is moving into.
        if (excludedReservationId.HasValue)
            Query.Where(reservation => reservation.ReservationId != excludedReservationId.Value);
    }
}
