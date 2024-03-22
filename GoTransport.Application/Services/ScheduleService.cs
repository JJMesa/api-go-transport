using AutoMapper;
using GoTransport.Application.Attributes;
using GoTransport.Application.Builders;
using GoTransport.Application.Commons;
using GoTransport.Application.Dtos.Schedule;
using GoTransport.Application.Interfaces;
using GoTransport.Application.Interfaces.Base;
using GoTransport.Application.Specifications.Schedules;
using GoTransport.Application.Wrappers;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Services;

[Transient]
internal class ScheduleService : IScheduleService
{
    private readonly IRepository<Schedule> _scheduleRepository;
    private readonly IMapper _mapper;

    public ScheduleService(IRepository<Schedule> scheduleRepository, IMapper mapper)
    {
        _scheduleRepository = scheduleRepository;
        _mapper = mapper;
    }

    public async Task<JsonResponse<IEnumerable<ScheduleDto>>> GetAllByRouteAsync(int ruoteId, CancellationToken cancellationToken)
    {
        var schedules = await _scheduleRepository.ListAsync(new ScheduleSpecification(ruoteId), cancellationToken);
        return ResponseBuilder<IEnumerable<ScheduleDto>>.Ok(_mapper.Map<IEnumerable<ScheduleDto>>(schedules));
    }

    public async Task<JsonResponse<ScheduleDto>> GetByIdAsync(dynamic id, CancellationToken cancellationToken)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(id, cancellationToken);
        if (schedule is null) return ResponseBuilder<ScheduleDto>.NotFound();
        return ResponseBuilder<ScheduleDto>.Ok(_mapper.Map<ScheduleDto>(schedule));
    }

    public async Task<JsonResponse<ScheduleDto>> CreateAsync(ScheduleCreationDto scheduleCreation)
    {
        var schedule = _mapper.Map<Schedule>(scheduleCreation);
        schedule.Duration = schedule.ArrivalTime - schedule.DepartureTime;

        await _scheduleRepository.AddAsync(schedule);

        return ResponseBuilder<ScheduleDto>.Created(_mapper.Map<ScheduleDto>(schedule));
    }

    public async Task<JsonResponse<ScheduleDto>> UpdateAsync(dynamic id, ScheduleUpdateDto scheduleUpdate)
    {
        if (id != scheduleUpdate.ScheduleId)
            return ResponseBuilder<ScheduleDto>.BadRequest(ErrorMessages.UrlAndBodyIdNotEqual);

        var schedule = await _scheduleRepository.GetByIdAsync(id);
        if (schedule is null) return ResponseBuilder<ScheduleDto>.NotFound();

        schedule = _mapper.Map(scheduleUpdate, schedule);
        schedule.Duration = schedule.ArrivalTime - schedule.DepartureTime;

        await _scheduleRepository.UpdateAsync(schedule);

        return ResponseBuilder<ScheduleDto>.Ok(_mapper.Map<ScheduleDto>(schedule));
    }

    public async Task<JsonResponse<bool?>> DeleteAsync(dynamic id)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(id);
        if (schedule is null) return ResponseBuilder<bool?>.NotFound();

        await _scheduleRepository.DeleteAsync(schedule);
        return ResponseBuilder<bool?>.NoContent();
    }
}