using GoTransport.Application.Builders;
using GoTransport.Application.Commons;
using GoTransport.Application.Dtos.Schedule;
using GoTransport.Application.Extensions;
using GoTransport.Application.Interfaces;
using GoTransport.Application.Interfaces.Base;
using GoTransport.Application.Mappings;
using GoTransport.Application.Specifications.Schedules;
using GoTransport.Application.Wrappers;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Services;

internal class ScheduleService : IScheduleService
{
    private readonly IRepository<Schedule> _scheduleRepository;

    public ScheduleService(IRepository<Schedule> scheduleRepository)
    {
        _scheduleRepository = scheduleRepository;
    }

    public async Task<JsonResponse<IEnumerable<ScheduleDto>>> GetAllByRouteAsync(int routeId, CancellationToken cancellationToken)
    {
        var schedules = await _scheduleRepository.ListAsync(new ScheduleSpecification(routeId), cancellationToken);
        return ResponseBuilder<IEnumerable<ScheduleDto>>.Ok(schedules.Select(schedule => schedule.ToDto()).ToList());
    }

    public async Task<JsonResponse<ScheduleDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var schedule = await _scheduleRepository.FirstOrDefaultAsync(new ScheduleSpecification(id), cancellationToken);
        if (schedule is null) return ResponseBuilder<ScheduleDto>.NotFound();
        return ResponseBuilder<ScheduleDto>.Ok(schedule.ToDto());
    }

    public async Task<JsonResponse<ScheduleDto>> CreateAsync(ScheduleCreationDto scheduleCreation, CancellationToken cancellationToken)
    {
        var schedule = scheduleCreation.ToEntity();
        schedule.Duration = ScheduleExtensions.TripDuration(schedule.DepartureTime, schedule.ArrivalTime);

        await _scheduleRepository.AddAsync(schedule, cancellationToken);

        // Reload with the navigations, so the response has the same shape as a get-by-id.
        schedule = await _scheduleRepository.FirstOrDefaultAsync(new ScheduleSpecification(schedule.ScheduleId), cancellationToken) ?? schedule;

        return ResponseBuilder<ScheduleDto>.Created(schedule.ToDto());
    }

    public async Task<JsonResponse<ScheduleDto>> UpdateAsync(Guid id, ScheduleUpdateDto scheduleUpdate, CancellationToken cancellationToken)
    {
        if (id != scheduleUpdate.ScheduleId)
            return ResponseBuilder<ScheduleDto>.BadRequest(ErrorMessages.UrlAndBodyIdNotEqual);

        var schedule = await _scheduleRepository.GetByIdAsync(id, cancellationToken);
        if (schedule is null) return ResponseBuilder<ScheduleDto>.NotFound();

        scheduleUpdate.ApplyTo(schedule);
        schedule.Duration = ScheduleExtensions.TripDuration(schedule.DepartureTime, schedule.ArrivalTime);

        await _scheduleRepository.UpdateAsync(schedule, cancellationToken);

        return ResponseBuilder<ScheduleDto>.Ok(schedule.ToDto());
    }

    public async Task<JsonResponse<bool?>> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(id, cancellationToken);
        if (schedule is null) return ResponseBuilder<bool?>.NotFound();

        await _scheduleRepository.DeleteAsync(schedule, cancellationToken);
        return ResponseBuilder<bool?>.NoContent();
    }
}