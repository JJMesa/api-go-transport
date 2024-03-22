using GoTransport.Application.Dtos.Reservation;
using GoTransport.Application.Interfaces.Base;
using GoTransport.Application.Parameters;
using GoTransport.Application.Wrappers;

namespace GoTransport.Application.Interfaces;

public interface IReservationService : IServiceBase<ReservationCreationDto, ReservationUpdateDto, ReservationDto>
{
    Task<JsonPagedResponse<IEnumerable<ReservationDto>>> GetByScheduleAsync(Guid scheduleId, ReservationParameters parameters, CancellationToken cancellationToken);

    Task<JsonResponse<ReservationDto>> GetDetailsByPerson(string id, string passengerIdentification, CancellationToken cancellationToken);
}