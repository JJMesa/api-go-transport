using GoTransport.Application.Dtos.Department;
using GoTransport.Application.Interfaces.Base;
using GoTransport.Application.Parameters;
using GoTransport.Application.Wrappers;

namespace GoTransport.Application.Interfaces;

public interface IDepartmentService : IServiceBase<DepartmentCreationDto, DepartmentUpdateDto, DepartmentDto>
{
    Task<JsonPagedResponse<IEnumerable<DepartmentDto>>> GetAsync(DepartmentParameters parameters, CancellationToken cancellationToken);
}