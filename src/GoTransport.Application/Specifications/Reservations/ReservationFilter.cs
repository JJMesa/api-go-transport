using Ardalis.Specification;
using GoTransport.Application.Extensions;
using GoTransport.Application.Parameters;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Specifications.Reservations;

/// <summary>
/// The filters a page of Reservation rows and its total count share, so the page and the count always agree.
/// </summary>
internal static class ReservationFilter
{
    public static void Apply(ISpecificationBuilder<Reservation> query, Guid scheduleId, ReservationParameters parameters)
    {
        query.Where(reservation => reservation.ScheduleId == scheduleId);

        if (!string.IsNullOrWhiteSpace(parameters.SearchCriteria))
        {
            query.Search(x => $"{x.PassengerFirstName}", parameters.SearchCriteria.ToContainsPattern());
            query.Search(x => $"{x.PassengerLastName}", parameters.SearchCriteria.ToContainsPattern());
            query.Search(x => $"{x.PassengerIdentification}", parameters.SearchCriteria.ToContainsPattern());
        }

        if (parameters.IsActive.HasValue)
            query.Where(reservation => reservation.IsActive == parameters.IsActive);

        if (parameters.TravelDate.HasValue)
        {
            var travelDate = parameters.TravelDate.Value.Date;
            query.Where(reservation => reservation.ReservationDate == travelDate);
        }
    }
}
