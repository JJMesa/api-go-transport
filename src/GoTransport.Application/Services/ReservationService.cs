using System.Data;
using GoTransport.Application.Attributes;
using GoTransport.Application.Builders;
using GoTransport.Application.Commons;
using GoTransport.Application.Dtos.Reservation;
using GoTransport.Application.Interfaces;
using GoTransport.Application.Interfaces.Base;
using GoTransport.Application.Mappings;
using GoTransport.Application.Parameters;
using GoTransport.Application.Specifications.Reservations;
using GoTransport.Application.Specifications.Schedules;
using GoTransport.Application.Wrappers;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Services;

[Transient]
internal class ReservationService : IReservationService
{
    private readonly IRepository<Reservation> _reservationRepository;
    private readonly IRepository<Schedule> _scheduleRepository;
    private readonly ICacheService<Reservation> _cacheService;
    private readonly IUnitOfWork _unitOfWork;

    public ReservationService(IRepository<Reservation> reservationRepository
        , IRepository<Schedule> scheduleRepository
        , ICacheService<Reservation> cacheService
        , IUnitOfWork unitOfWork)
    {
        _reservationRepository = reservationRepository;
        _scheduleRepository = scheduleRepository;
        _cacheService = cacheService;
        _unitOfWork = unitOfWork;
    }

    public async Task<JsonResponse<IEnumerable<ReservationDto>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var reservations = _cacheService.Get(CacheKey.Reservations);
        if (reservations is null)
        {
            reservations = await _reservationRepository.ListAsync(new ReservationSpecification(), cancellationToken);
            _cacheService.Set(CacheKey.Reservations, reservations);
        }
        return ResponseBuilder<IEnumerable<ReservationDto>>.Ok(reservations.Select(reservation => reservation.ToDto()).ToList());
    }

    public async Task<JsonPagedResponse<IEnumerable<ReservationDto>>> GetByScheduleAsync(Guid scheduleId, ReservationParameters parameters, CancellationToken cancellationToken)
    {
        var reservations = await _reservationRepository.ListAsync(new PagedReservationSpecification(scheduleId, parameters), cancellationToken);
        var totalRecords = await _reservationRepository.CountAsync(new ReservationSpecification(scheduleId, parameters), cancellationToken);
        var metadata = Metadata.Create(parameters.PageNumber, parameters.PageSize, totalRecords);
        return ResponseBuilder<IEnumerable<ReservationDto>>.OkPaged(reservations, metadata);
    }

    public async Task<JsonResponse<ReservationDto>> GetDetailsByPerson(string id, string passengerIdentification, CancellationToken cancellationToken)
    {
        var reservation = await _reservationRepository.FirstOrDefaultAsync(new ReservationSpecification(id, passengerIdentification), cancellationToken);
        if (reservation is null) return ResponseBuilder<ReservationDto>.NotFound();
        return ResponseBuilder<ReservationDto>.Ok(reservation.ToDto());
    }

    public async Task<JsonResponse<ReservationDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var reservation = await _reservationRepository.GetByIdAsync(id, cancellationToken);
        if (reservation is null) return ResponseBuilder<ReservationDto>.NotFound();
        return ResponseBuilder<ReservationDto>.Ok(reservation.ToDto());
    }

    public Task<JsonResponse<ReservationDto>> CreateAsync(ReservationCreationDto reservationCreation) =>
        // Serializable keeps the count and the insert atomic: a concurrent booking on the same Trip waits,
        // or fails and is retried, instead of taking a place this count has already handed out.
        _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var schedule = await _scheduleRepository.FirstOrDefaultAsync(new ScheduleSpecification(reservationCreation.ScheduleId));
            if (schedule is null)
                return ResponseBuilder<ReservationDto>.NotFound(ErrorMessages.InvalidSchedule);

            var placesTaken = await _reservationRepository.CountAsync(
                new TripReservationSpecification(reservationCreation.ScheduleId, reservationCreation.ReservationDate));

            if (IsVehicleFull(placesTaken, schedule.Vehicle.Capacity))
                return ResponseBuilder<ReservationDto>.Conflict(ErrorMessages.VehicleFull);

            var reservation = reservationCreation.ToEntity();
            await _reservationRepository.AddAsync(reservation);
            return ResponseBuilder<ReservationDto>.Ok(reservation.ToDto());
        }, IsolationLevel.Serializable);

    public async Task<JsonResponse<ReservationDto>> UpdateAsync(Guid id, ReservationUpdateDto reservationUpdateDto)
    {
        if (id != reservationUpdateDto.ReservationId)
            return ResponseBuilder<ReservationDto>.BadRequest(ErrorMessages.UrlAndBodyIdNotEqual);

        var reservation = await _reservationRepository.FirstOrDefaultAsync(new ReservationSpecification(reservationId: id));
        if (reservation is null) return ResponseBuilder<ReservationDto>.NotFound();

        if (IsReservationDatePassed(reservation))
            return ResponseBuilder<ReservationDto>.BadRequest(ErrorMessages.ReservationDatePassed);

        reservationUpdateDto.ApplyTo(reservation);
        await _reservationRepository.UpdateAsync(reservation);
        return ResponseBuilder<ReservationDto>.Ok(reservation.ToDto());
    }

    public async Task<JsonResponse<bool?>> DeleteAsync(Guid id)
    {
        // The "trip passed" check reads the Schedule, so load it with the Reservation.
        var reservation = await _reservationRepository.FirstOrDefaultAsync(new ReservationSpecification(reservationId: id));
        if (reservation is null) return ResponseBuilder<bool?>.NotFound();

        if (IsReservationDatePassed(reservation))
            return ResponseBuilder<bool?>.BadRequest(ErrorMessages.ReservationDatePassed);

        await _reservationRepository.DeleteAsync(reservation);
        return ResponseBuilder<bool?>.NoContent();
    }

    private static bool IsVehicleFull(int reservationsCount, int vehicleCapacity) =>
        reservationsCount >= vehicleCapacity;

    private static bool IsReservationDatePassed(Reservation reservation) =>
        reservation.ReservationDate.Date + reservation.Schedule.ArrivalTime < DateTime.Now;
}