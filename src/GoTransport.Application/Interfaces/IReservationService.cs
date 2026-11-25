using GoTransport.Application.Dtos.Reservation;
using GoTransport.Application.Parameters;
using GoTransport.Application.Wrappers;

namespace GoTransport.Application.Interfaces;

/// <summary>
/// Reservations have no unbounded "all" list (F-38): they are listed per Schedule, optionally per Trip.
/// </summary>
public interface IReservationService
{
    Task<JsonResponse<ReservationDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<JsonResponse<ReservationDto>> CreateAsync(ReservationCreationDto reservationCreation);

    Task<JsonResponse<ReservationDto>> UpdateAsync(Guid id, ReservationUpdateDto reservationUpdate);

    Task<JsonResponse<bool?>> DeleteAsync(Guid id);

    Task<JsonPagedResponse<IEnumerable<ReservationDto>>> GetByScheduleAsync(Guid scheduleId, ReservationParameters parameters, CancellationToken cancellationToken);

    Task<JsonResponse<ReservationDto>> GetDetailsByPerson(string id, string passengerIdentification, CancellationToken cancellationToken);
}