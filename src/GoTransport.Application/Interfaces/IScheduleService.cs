using GoTransport.Application.Dtos.Schedule;
using GoTransport.Application.Wrappers;

namespace GoTransport.Application.Interfaces;

public interface IScheduleService
{
    Task<JsonResponse<IEnumerable<ScheduleDto>>> GetAllByRouteAsync(int routeId, CancellationToken cancellationToken);

    Task<JsonResponse<ScheduleDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<JsonResponse<ScheduleDto>> CreateAsync(ScheduleCreationDto entity);

    Task<JsonResponse<ScheduleDto>> UpdateAsync(Guid id, ScheduleUpdateDto entity);

    Task<JsonResponse<bool?>> DeleteAsync(Guid id);
}