using GoTransport.Application.Dtos.Department;
using GoTransport.Application.Parameters;
using GoTransport.Application.Wrappers;

namespace GoTransport.Application.Interfaces;

public interface IDepartmentService
{
    /// <summary>
    /// Every active Department, served from the in-memory cache.
    /// </summary>
    Task<JsonResponse<IEnumerable<DepartmentDto>>> GetAllAsync(CancellationToken cancellationToken);

    Task<JsonResponse<DepartmentDto>> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<JsonResponse<DepartmentDto>> CreateAsync(DepartmentCreationDto departmentCreation);

    Task<JsonResponse<DepartmentDto>> UpdateAsync(int id, DepartmentUpdateDto departmentUpdate);

    Task<JsonResponse<bool?>> DeleteAsync(int id);

    Task<JsonPagedResponse<IEnumerable<DepartmentDto>>> GetAsync(DepartmentParameters parameters, CancellationToken cancellationToken);
}