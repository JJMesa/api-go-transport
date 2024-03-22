using GoTransport.Application.Wrappers;

namespace GoTransport.Application.Interfaces.Base;

public interface IServiceBase<TCreate, TUpdate, TDto> where TCreate : class where TUpdate : class where TDto : class
{
    Task<JsonResponse<IEnumerable<TDto>>> GetAllAsync(CancellationToken cancellationToken);

    Task<JsonResponse<TDto>> GetByIdAsync(dynamic id, CancellationToken cancellationToken);

    Task<JsonResponse<TDto>> CreateAsync(TCreate entity);

    Task<JsonResponse<TDto>> UpdateAsync(dynamic id, TUpdate entity);

    Task<JsonResponse<bool?>> DeleteAsync(dynamic id);
}