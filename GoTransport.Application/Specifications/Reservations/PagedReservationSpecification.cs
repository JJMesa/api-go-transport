using Ardalis.Specification;
using GoTransport.Application.Dtos.City;
using GoTransport.Application.Dtos.IdentificationType;
using GoTransport.Application.Dtos.Point;
using GoTransport.Application.Dtos.Reservation;
using GoTransport.Application.Dtos.Route;
using GoTransport.Application.Dtos.Schedule;
using GoTransport.Application.Enums;
using GoTransport.Application.Parameters;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Specifications.Reservations;

public class PagedReservationSpecification : Specification<Reservation, ReservationDto>
{
    public PagedReservationSpecification(Guid scheduleId, ReservationParameters parameters)
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

        Query.Skip((parameters.PageNumber - 1) * parameters.PageSize).Take(parameters.PageSize);

        Query.Select(reservation => new ReservationDto
        {
            ReservationId = reservation.ReservationId,
            Schedule = new ScheduleDto
            {
                ScheduleId = reservation.Schedule.ScheduleId,
                ArrivalTime = reservation.Schedule.ArrivalTime,
                DepartureTime = reservation.Schedule.DepartureTime,
                Duration = reservation.Schedule.Duration,
                Route = new RouteDto
                {
                    RouteId = reservation.Schedule.Route.RouteId,
                    Description = reservation.Schedule.Route.Description,
                    OriginPoint = new PointDto
                    {
                        PointId = reservation.Schedule.Route.OriginPoint.PointId,
                        Detail = reservation.Schedule.Route.OriginPoint.Detail,
                        City = new CityDto
                        {
                            CityId = reservation.Schedule.Route.OriginPoint.City.CityId,
                            Description = reservation.Schedule.Route.OriginPoint.City.Description,
                            IsActive = reservation.Schedule.Route.OriginPoint.City.IsActive!.Value
                        }
                    },
                    DestinationPoint = new PointDto
                    {
                        PointId = reservation.Schedule.Route.DestinationPoint.PointId,
                        Detail = reservation.Schedule.Route.DestinationPoint.Detail,
                        City = new CityDto
                        {
                            CityId = reservation.Schedule.Route.DestinationPoint.City.CityId,
                            Description = reservation.Schedule.Route.DestinationPoint.City.Description,
                            IsActive = reservation.Schedule.Route.DestinationPoint.City.IsActive!.Value
                        }
                    },
                    IsActive = reservation.Schedule.Route.IsActive!.Value
                }
            },
            PassengerFirstName = reservation.PassengerFirstName,
            PassengerLastName = reservation.PassengerLastName,
            IdentificationType = new IdentificationTypeDto
            {
                IdentificationTypeId = reservation.IdentificationType.IdentificationTypeId,
                Description = reservation.IdentificationType.Description
            },
            PassengerIdentification = reservation.PassengerIdentification,
            Detail = reservation.Detail,
            IsActive = reservation.IsActive!.Value
        });
    }
}