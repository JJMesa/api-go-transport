using GoTransport.Application.Dtos.Reservation;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Mappings;

internal static class ReservationMappingExtensions
{
    /// <summary>
    /// Maps a reservation to its DTO. A navigation property that was not loaded maps to null.
    /// </summary>
    public static ReservationDto ToDto(this Reservation reservation) => new()
    {
        ReservationId = reservation.ReservationId,
        Schedule = reservation.Schedule?.ToDto()!,
        PassengerFirstName = reservation.PassengerFirstName,
        PassengerLastName = reservation.PassengerLastName,
        IdentificationType = reservation.IdentificationType?.ToDto()!,
        PassengerIdentification = reservation.PassengerIdentification,
        Detail = reservation.Detail,
        IsActive = reservation.IsActive ?? false
    };

    public static Reservation ToEntity(this ReservationCreationDto reservationCreation) => new()
    {
        ScheduleId = reservationCreation.ScheduleId,
        ReservationDate = reservationCreation.ReservationDate,
        PassengerFirstName = reservationCreation.PassengerFirstName,
        PassengerLastName = reservationCreation.PassengerLastName,
        IdentificationTypeId = reservationCreation.IdentificationTypeId,
        PassengerIdentification = reservationCreation.PassengerIdentification,
        Detail = reservationCreation.Detail
    };

    public static void ApplyTo(this ReservationUpdateDto reservationUpdate, Reservation reservation)
    {
        reservation.ScheduleId = reservationUpdate.ScheduleId;
        reservation.ReservationDate = reservationUpdate.ReservationDate;
        reservation.Detail = reservationUpdate.Detail;
    }
}
