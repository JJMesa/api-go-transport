using GoTransport.Application.Dtos.City;
using GoTransport.Application.Parameters;
using GoTransport.Application.Wrappers;

namespace GoTransport.Application.Interfaces;

public interface ICityService
{
    Task<JsonPagedResponse<IEnumerable<CityDto>>> GetAsync(CityParameters dto, CancellationToken cancellationToken);

    Task<JsonResponse<IEnumerable<CityDto>>> GetAllByDepartmentAsync(int departmentId, CancellationToken cancellationToken);

    Task<JsonResponse<CityDto>> GetByIdAsync(dynamic id, CancellationToken cancellationToken);

    Task<JsonResponse<CityDto>> CreateAsync(CityCreationDto dto);

    Task<JsonResponse<CityDto>> UpdateAsync(dynamic id, CityUpdateDto dto);

    Task<JsonResponse<bool?>> DeleteAsync(dynamic id);
}