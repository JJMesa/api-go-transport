using Ardalis.Specification;
using GoTransport.Application.Enums;
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
    }

    public ReservationSpecification(Guid? scheduleId = null, Guid? reservationId = null)
    {
        Query.Where(reservation => reservation.ScheduleId == scheduleId || scheduleId == null);

        if (reservationId.HasValue)
        {
            Query.Where(reservation => reservation.ReservationId == reservationId || reservationId == null);
            Query.Include(reservation => reservation.Schedule);
        }
    }

    public ReservationSpecification(Guid scheduleId, ReservationParameters parameters)
    {
        Query.Where(reservation => reservation.ScheduleId == scheduleId);

        if (!string.IsNullOrWhiteSpace(parameters.SearchCriteria))
        {
            Query.Search(x => $"{x.PassengerFirstName}", $"%{parameters.SearchCriteria}%");
            Query.Search(x => $"{x.PassengerLastName}", $"%{parameters.SearchCriteria}%");
            Query.Search(x => $"{x.PassengerIdentification}", $"%{parameters.SearchCriteria}%");
        }

        switch (parameters.OrderBy)
        {
            case SortDirection.Ascending:
                Query.OrderBy(schedule => schedule.PassengerFirstName)
                    .ThenBy(schedule => schedule.PassengerLastName);
                break;

            case SortDirection.Descending:
                Query.OrderByDescending(schedule => schedule.PassengerFirstName)
                    .ThenByDescending(schedule => schedule.PassengerLastName);
                break;

            default:
                Query.OrderBy(schedule => schedule.PassengerFirstName)
                    .ThenBy(schedule => schedule.PassengerLastName);
                break;
        }

        if (parameters.IsActive.HasValue)
            Query.Where(schedule => schedule.IsActive == parameters.IsActive);

        if (parameters.TravelDate.HasValue)
        {
            var travelDate = parameters.TravelDate.Value.Date;
            Query.Where(reservation => reservation.ReservationDate == travelDate);
        }
    }
}