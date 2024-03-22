using GoTransport.Application.Dtos.IdentificationType;
using GoTransport.Application.Wrappers;

namespace GoTransport.Application.Interfaces;

public interface IIdentificationTypeService
{
    Task<JsonResponse<IEnumerable<IdentificationTypeDto>>> GetAllAsync(CancellationToken cancellationToken);
}