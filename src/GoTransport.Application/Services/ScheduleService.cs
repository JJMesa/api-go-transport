using GoTransport.Application.Attributes;
using GoTransport.Application.Builders;
using GoTransport.Application.Commons;
using GoTransport.Application.Dtos.Schedule;
using GoTransport.Application.Interfaces;
using GoTransport.Application.Interfaces.Base;
using GoTransport.Application.Mappings;
using GoTransport.Application.Specifications.Schedules;
using GoTransport.Application.Wrappers;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Services;

[Transient]
internal class ScheduleService : IScheduleService
{
    private readonly IRepository<Schedule> _scheduleRepository;

    public ScheduleService(IRepository<Schedule> scheduleRepository)
    {
        _scheduleRepository = scheduleRepository;
    }

    public async Task<JsonResponse<IEnumerable<ScheduleDto>>> GetAllByRouteAsync(int ruoteId, CancellationToken cancellationToken)
    {
        var schedules = await _scheduleRepository.ListAsync(new ScheduleSpecification(ruoteId), cancellationToken);
        return ResponseBuilder<IEnumerable<ScheduleDto>>.Ok(schedules.Select(schedule => schedule.ToDto()).ToList());
    }

    public async Task<JsonResponse<ScheduleDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(id, cancellationToken);
        if (schedule is null) return ResponseBuilder<ScheduleDto>.NotFound();
        return ResponseBuilder<ScheduleDto>.Ok(schedule.ToDto());
    }

    public async Task<JsonResponse<ScheduleDto>> CreateAsync(ScheduleCreationDto scheduleCreation)
    {
        var schedule = scheduleCreation.ToEntity();
        schedule.Duration = schedule.ArrivalTime - schedule.DepartureTime;

        await _scheduleRepository.AddAsync(schedule);

        return ResponseBuilder<ScheduleDto>.Created(schedule.ToDto());
    }

    public async Task<JsonResponse<ScheduleDto>> UpdateAsync(Guid id, ScheduleUpdateDto scheduleUpdate)
    {
        if (id != scheduleUpdate.ScheduleId)
            return ResponseBuilder<ScheduleDto>.BadRequest(ErrorMessages.UrlAndBodyIdNotEqual);

        var schedule = await _scheduleRepository.GetByIdAsync(id);
        if (schedule is null) return ResponseBuilder<ScheduleDto>.NotFound();

        scheduleUpdate.ApplyTo(schedule);
        schedule.Duration = schedule.ArrivalTime - schedule.DepartureTime;

        await _scheduleRepository.UpdateAsync(schedule);

        return ResponseBuilder<ScheduleDto>.Ok(schedule.ToDto());
    }

    public async Task<JsonResponse<bool?>> DeleteAsync(Guid id)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(id);
        if (schedule is null) return ResponseBuilder<bool?>.NotFound();

        await _scheduleRepository.DeleteAsync(schedule);
        return ResponseBuilder<bool?>.NoContent();
    }
}