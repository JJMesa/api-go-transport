using GoTransport.Application.Dtos.Manufacturer;
using GoTransport.Application.Wrappers;

namespace GoTransport.Application.Interfaces;

public interface IManufacturerService
{
    Task<JsonResponse<IEnumerable<ManufacturerDto>>> GetAllAsync(CancellationToken cancellationToken);
}