using GoTransport.Application.Wrappers;

namespace GoTransport.Application.Interfaces.Base;

public interface IServiceBase<TKey, TCreate, TUpdate, TDto>
    where TKey : notnull
    where TCreate : class
    where TUpdate : class
    where TDto : class
{
    Task<JsonResponse<IEnumerable<TDto>>> GetAllAsync(CancellationToken cancellationToken);

    Task<JsonResponse<TDto>> GetByIdAsync(TKey id, CancellationToken cancellationToken);

    Task<JsonResponse<TDto>> CreateAsync(TCreate entity);

    Task<JsonResponse<TDto>> UpdateAsync(TKey id, TUpdate entity);

    Task<JsonResponse<bool?>> DeleteAsync(TKey id);
}
