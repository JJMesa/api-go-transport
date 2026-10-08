using GoTransport.Api.Test.Utilities.Commons;
using GoTransport.Application.Dtos.Reservation;
using GoTransport.Application.Enums;
using GoTransport.Application.Parameters;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Api.Test.Utilities.Mothers;

public static class ReservationBuilderMother
{
    public static ReservationParameters ReservationParameters(int pageNumber = 1, int pageSize = 5, string? search = null, SortDirection? sort = null, bool? isActive = null)
    {
        return new ReservationParameters
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchCriteria = search,
            OrderBy = sort,
            IsActive = isActive
        };
    }

    public static Schedule ScheduleEntity(Guid? scheduleId = null, TimeSpan? arrivalTime = null)
    {
        return new Schedule
        {
            ScheduleId = scheduleId ?? Guid.NewGuid(),
            DepartureTime = new TimeSpan(8, 0, 0),
            ArrivalTime = arrivalTime ?? new TimeSpan(10, 0, 0),
            Duration = new TimeSpan(2, 0, 0),
            IsActive = true
        };
    }

    public static Reservation ReservationEntity(Guid reservationId, DateTime reservationDate, Schedule? schedule = null)
    {
        var scheduleEntity = schedule ?? ScheduleEntity();

        return new Reservation
        {
            ReservationId = reservationId,
            ScheduleId = scheduleEntity.ScheduleId,
            Schedule = scheduleEntity,
            ReservationDate = reservationDate,
            PassengerFirstName = "John",
            PassengerLastName = "Doe",
            IdentificationTypeId = Utils.DefaultId,
            PassengerIdentification = "123456789",
            Detail = "Test reservation",
            IsActive = true
        };
    }

    public static ReservationCreationDto ReservationCreationDtoOk(Guid scheduleId)
    {
        return new ReservationCreationDto
        {
            ScheduleId = scheduleId,
            ReservationDate = DateTime.Now.Date.AddDays(1),
            PassengerFirstName = "John",
            PassengerLastName = "Doe",
            IdentificationTypeId = Utils.DefaultId,
            PassengerIdentification = "123456789",
            Detail = "Test reservation"
        };
    }

    public static ReservationUpdateDto ReservationUpdateDtoOk(Guid reservationId, Guid scheduleId)
    {
        return new ReservationUpdateDto
        {
            ReservationId = reservationId,
            ScheduleId = scheduleId,
            ReservationDate = DateTime.Now.Date.AddDays(1),
            Detail = "Updated reservation"
        };
    }
}
