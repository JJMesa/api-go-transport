using Ardalis.Specification;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Specifications.Reservations;

/// <summary>
/// One Reservation with its Trip (Schedule, Route, Points, Cities) and identification type, the same
/// shape the paged per-Schedule list projects.
/// </summary>
public class ReservationDetailSpecification : Specification<Reservation>
{
    public ReservationDetailSpecification(Guid reservationId)
    {
        Query.Where(reservation => reservation.ReservationId == reservationId);

        Query.Include(reservation => reservation.Schedule.Route.OriginPoint.City);
        Query.Include(reservation => reservation.Schedule.Route.DestinationPoint.City);
        Query.Include(reservation => reservation.IdentificationType);

        // Read-only: nothing loaded here is saved back, so skip change tracking.
        Query.AsNoTracking();
    }
}
