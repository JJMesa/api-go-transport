using Ardalis.Specification;
using GoTransport.Application.Parameters;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Specifications.Reservations;

public class ReservationSpecification : Specification<Reservation>
{
    /// <summary>
    /// The Reservation with this id, only if it belongs to the Passenger with this document number.
    /// An exact key match, so the lookup uses the primary key instead of scanning the table.
    /// </summary>
    public ReservationSpecification(Guid reservationId, string passengerIdentification)
    {
        Query.Where(reservation =>
            reservation.ReservationId == reservationId &&
            reservation.PassengerIdentification == passengerIdentification);

        Query.Include(reservation => reservation.Schedule.Route.OriginPoint.City);
        Query.Include(reservation => reservation.Schedule.Route.DestinationPoint.City);
        Query.Include(reservation => reservation.IdentificationType);

        // Read-only: nothing loaded here is saved back, so skip change tracking.
        Query.AsNoTracking();
    }

    public ReservationSpecification(Guid? scheduleId = null, Guid? reservationId = null)
    {
        if (scheduleId.HasValue)
            Query.Where(reservation => reservation.ScheduleId == scheduleId.Value);

        if (reservationId.HasValue)
        {
            Query.Where(reservation => reservation.ReservationId == reservationId.Value);
            Query.Include(reservation => reservation.Schedule);
        }
    }

    public ReservationSpecification(Guid scheduleId, ReservationParameters parameters)
    {
        ReservationFilter.Apply(Query, scheduleId, parameters);
    }
}