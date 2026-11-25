using System.Data;
using GoTransport.Application.Builders;
using GoTransport.Application.Commons;
using GoTransport.Application.Dtos.Reservation;
using GoTransport.Application.Extensions;
using GoTransport.Application.Interfaces;
using GoTransport.Application.Interfaces.Base;
using GoTransport.Application.Mappings;
using GoTransport.Application.Parameters;
using GoTransport.Application.Specifications.Reservations;
using GoTransport.Application.Specifications.Schedules;
using GoTransport.Application.Wrappers;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Services;

internal class ReservationService : IReservationService
{
    private readonly IRepository<Reservation> _reservationRepository;
    private readonly IRepository<Schedule> _scheduleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReservationService(IRepository<Reservation> reservationRepository
        , IRepository<Schedule> scheduleRepository
        , IUnitOfWork unitOfWork)
    {
        _reservationRepository = reservationRepository;
        _scheduleRepository = scheduleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<JsonPagedResponse<IEnumerable<ReservationDto>>> GetByScheduleAsync(Guid scheduleId, ReservationParameters parameters, CancellationToken cancellationToken)
    {
        var reservations = await _reservationRepository.ListAsync(new PagedReservationSpecification(scheduleId, parameters), cancellationToken);
        var totalRecords = await _reservationRepository.CountAsync(new ReservationSpecification(scheduleId, parameters), cancellationToken);
        var metadata = Metadata.Create(parameters.PageNumber, parameters.PageSize, totalRecords);
        return ResponseBuilder<IEnumerable<ReservationDto>>.OkPaged(reservations, metadata);
    }

    public async Task<JsonResponse<ReservationDto>> GetByPassengerAsync(Guid id, string passengerIdentification, CancellationToken cancellationToken)
    {
        var reservation = await _reservationRepository.FirstOrDefaultAsync(new ReservationSpecification(id, passengerIdentification), cancellationToken);
        if (reservation is null) return ResponseBuilder<ReservationDto>.NotFound();
        return ResponseBuilder<ReservationDto>.Ok(reservation.ToDto());
    }

    public async Task<JsonResponse<ReservationDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var reservation = await _reservationRepository.FirstOrDefaultAsync(new ReservationDetailSpecification(id), cancellationToken);
        if (reservation is null) return ResponseBuilder<ReservationDto>.NotFound();
        return ResponseBuilder<ReservationDto>.Ok(reservation.ToDto());
    }

    public Task<JsonResponse<ReservationDto>> CreateAsync(ReservationCreationDto reservationCreation, CancellationToken cancellationToken) =>
        // Serializable keeps the count and the insert atomic: a concurrent booking on the same Trip waits,
        // or fails and is retried, instead of taking a place this count has already handed out.
        _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var tripCheck = await CheckTripCanTakeAsync(reservationCreation.ScheduleId, reservationCreation.ReservationDate, cancellationToken: cancellationToken);
            if (tripCheck is not null) return tripCheck;

            var reservation = reservationCreation.ToEntity();
            await _reservationRepository.AddAsync(reservation, cancellationToken);

            // Reload with the navigations, so the response has the same shape as a get-by-id.
            reservation = await _reservationRepository.FirstOrDefaultAsync(new ReservationDetailSpecification(reservation.ReservationId), cancellationToken) ?? reservation;
            return ResponseBuilder<ReservationDto>.Created(reservation.ToDto());
        }, IsolationLevel.Serializable, cancellationToken);

    public async Task<JsonResponse<ReservationDto>> UpdateAsync(Guid id, ReservationUpdateDto reservationUpdateDto, CancellationToken cancellationToken)
    {
        if (id != reservationUpdateDto.ReservationId)
            return ResponseBuilder<ReservationDto>.BadRequest(ErrorMessages.UrlAndBodyIdNotEqual);

        // Same isolation as a booking: moving onto a Trip takes one of its places.
        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var reservation = await _reservationRepository.FirstOrDefaultAsync(new ReservationSpecification(reservationId: id), cancellationToken);

            // A cancelled Reservation is kept for the record but can no longer change.
            if (reservation is null || reservation.IsActive != true) return ResponseBuilder<ReservationDto>.NotFound();

            if (HasTripEnded(reservation.ReservationDate, reservation.Schedule))
                return ResponseBuilder<ReservationDto>.BadRequest(ErrorMessages.ReservationDatePassed);

            // Moving to another Trip must pass the same checks as a new booking on it.
            if (IsMovingToAnotherTrip(reservation, reservationUpdateDto))
            {
                var tripCheck = await CheckTripCanTakeAsync(reservationUpdateDto.ScheduleId, reservationUpdateDto.ReservationDate, movingReservationId: id, cancellationToken: cancellationToken);
                if (tripCheck is not null) return tripCheck;
            }

            reservationUpdateDto.ApplyTo(reservation);
            await _reservationRepository.UpdateAsync(reservation, cancellationToken);
            return ResponseBuilder<ReservationDto>.Ok(reservation.ToDto());
        }, IsolationLevel.Serializable, cancellationToken);
    }

    public async Task<JsonResponse<bool?>> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        // The "trip passed" check reads the Schedule, so load it with the Reservation.
        var reservation = await _reservationRepository.FirstOrDefaultAsync(new ReservationSpecification(reservationId: id), cancellationToken);
        if (reservation is null || reservation.IsActive != true) return ResponseBuilder<bool?>.NotFound();

        if (HasTripEnded(reservation.ReservationDate, reservation.Schedule))
            return ResponseBuilder<bool?>.BadRequest(ErrorMessages.ReservationDatePassed);

        // A cancellation keeps the Reservation on record as inactive. Only active Reservations count
        // against Capacity, so its place on the Trip is free again.
        reservation.IsActive = false;
        await _reservationRepository.UpdateAsync(reservation, cancellationToken);
        return ResponseBuilder<bool?>.NoContent();
    }

    /// <summary>
    /// Checks that a Reservation can be placed on the Trip (Schedule + travel date): the Schedule exists,
    /// the Trip has not taken place, and its Vehicle has a free place. Returns the failure, or null when it can.
    /// </summary>
    private async Task<JsonResponse<ReservationDto>?> CheckTripCanTakeAsync(Guid scheduleId, DateTime travelDate, Guid? movingReservationId = null, CancellationToken cancellationToken = default)
    {
        var schedule = await _scheduleRepository.FirstOrDefaultAsync(new ScheduleSpecification(scheduleId), cancellationToken);
        if (schedule is null)
            return ResponseBuilder<ReservationDto>.NotFound(ErrorMessages.InvalidSchedule);

        if (HasTripEnded(travelDate, schedule))
            return ResponseBuilder<ReservationDto>.BadRequest(ErrorMessages.TravelDatePassed);

        var placesTaken = await _reservationRepository.CountAsync(new TripReservationSpecification(scheduleId, travelDate, movingReservationId), cancellationToken);
        if (IsVehicleFull(placesTaken, schedule.Vehicle.Capacity))
            return ResponseBuilder<ReservationDto>.Conflict(ErrorMessages.VehicleFull);

        return null;
    }

    private static bool IsMovingToAnotherTrip(Reservation reservation, ReservationUpdateDto reservationUpdate) =>
        reservation.ScheduleId != reservationUpdate.ScheduleId
        || reservation.ReservationDate.Date != reservationUpdate.ReservationDate.Date;

    private static bool IsVehicleFull(int reservationsCount, int vehicleCapacity) =>
        reservationsCount >= vehicleCapacity;

    private static bool HasTripEnded(DateTime travelDate, Schedule schedule) =>
        schedule.ArrivalOn(travelDate) < DateTime.Now;
}