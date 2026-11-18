using Ardalis.Specification;
using GoTransport.Application.Enums;
using GoTransport.Application.Parameters;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Specifications.Reservations;

public class ReservationSpecification : Specification<Reservation>
{
    public ReservationSpecification()
    {
        Query.Where(reservation => reservation.IsActive!.Value);

        Query.OrderBy(reservation => reservation.PassengerFirstName)
            .ThenBy(reservation => reservation.PassengerLastName);
    }

    public ReservationSpecification(string reservationId, string personId)
    {
        Query.Where(reservation =>
            reservation.ReservationId.ToString().StartsWith(reservationId) &&
            reservation.PassengerIdentification == personId
        );
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
    }
}