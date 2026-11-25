using GoTransport.Application.Builders;
using GoTransport.Application.Commons;
using GoTransport.Application.Dtos.Department;
using GoTransport.Application.Interfaces;
using GoTransport.Domain.Interfaces;
using GoTransport.Application.Mappings;
using GoTransport.Application.Parameters;
using GoTransport.Application.Specifications.Departments;
using GoTransport.Application.Wrappers;
using GoTransport.Domain.Entities.Bas;

namespace GoTransport.Application.Services;

internal class DepartmentService : IDepartmentService
{
    private readonly IRepository<Department> _departmentRepository;
    private readonly ICacheService<Department> _cacheService;

    public DepartmentService(IRepository<Department> departmentRepository
        , ICacheService<Department> cacheService)
    {
        _departmentRepository = departmentRepository;
        _cacheService = cacheService;
    }

    public async Task<JsonResponse<IEnumerable<DepartmentDto>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var departments = _cacheService.Get(CacheKey.Departments);
        if (departments is null)
        {
            departments = await _departmentRepository.ListAsync(new DepartmentSpecification(), cancellationToken);
            _cacheService.Set(CacheKey.Departments, departments);
        }

        return ResponseBuilder<IEnumerable<DepartmentDto>>.Ok(departments.Select(department => department.ToDto()).ToList());
    }

    public async Task<JsonPagedResponse<IEnumerable<DepartmentDto>>> GetAsync(DepartmentParameters parameters, CancellationToken cancellationToken)
    {
        var departments = await _departmentRepository.ListAsync(new PagedDepartmentSpecification(parameters), cancellationToken);
        var totalAmount = await _departmentRepository.CountAsync(new DepartmentSpecification(parameters), cancellationToken);
        var metadata = Metadata.Create(parameters.PageNumber, parameters.PageSize, totalAmount);
        return ResponseBuilder<IEnumerable<DepartmentDto>>.OkPaged(departments, metadata);
    }

    public async Task<JsonResponse<DepartmentDto>> GetByIdAsync(int departmentId, CancellationToken cancellationToken)
    {
        var department = await _departmentRepository.GetByIdAsync(departmentId, cancellationToken);
        if (department is null) return ResponseBuilder<DepartmentDto>.NotFound();
        return ResponseBuilder<DepartmentDto>.Ok(department.ToDto());
    }

    public async Task<JsonResponse<DepartmentDto>> CreateAsync(DepartmentCreationDto departmentCreation, CancellationToken cancellationToken)
    {
        if (await IsDuplicateDescriptionAsync(departmentCreation.Description, cancellationToken: cancellationToken))
            return ResponseBuilder<DepartmentDto>.Conflict(ErrorMessages.DuplicateDescription);

        var department = departmentCreation.ToEntity();
        await _departmentRepository.AddAsync(department, cancellationToken);
        InvalidateCachedLists();

        return ResponseBuilder<DepartmentDto>.Created(department.ToDto());
    }

    public async Task<JsonResponse<DepartmentDto>> UpdateAsync(int id, DepartmentUpdateDto departmentUpdate, CancellationToken cancellationToken)
    {
        if (id != departmentUpdate.DepartmentId)
            return ResponseBuilder<DepartmentDto>.BadRequest(ErrorMessages.UrlAndBodyIdNotEqual);

        var department = await _departmentRepository.GetByIdAsync(id, cancellationToken);
        if (department is null) return ResponseBuilder<DepartmentDto>.NotFound();

        if (await IsDuplicateDescriptionAsync(departmentUpdate.Description, id, cancellationToken: cancellationToken))
            return ResponseBuilder<DepartmentDto>.Conflict(ErrorMessages.DuplicateDescription);

        departmentUpdate.ApplyTo(department);
        await _departmentRepository.UpdateAsync(department, cancellationToken);
        InvalidateCachedLists();
        return ResponseBuilder<DepartmentDto>.Ok(department.ToDto());
    }

    public async Task<JsonResponse<bool?>> DeleteAsync(int departmentId, CancellationToken cancellationToken)
    {
        var department = await _departmentRepository.GetByIdAsync(departmentId, cancellationToken);
        if (department is null) return ResponseBuilder<bool?>.NotFound();
        await _departmentRepository.DeleteAsync(department, cancellationToken);
        InvalidateCachedLists();
        return ResponseBuilder<bool?>.NoContent();
    }

    private async Task<bool> IsDuplicateDescriptionAsync(string description, int id = 0, CancellationToken cancellationToken = default) =>
        await _departmentRepository.AnyAsync(new DuplicationDepartmentSpecification(description, id), cancellationToken);

    /// <summary>
    /// Drops the cached Department list, so the next read shows this write.
    /// </summary>
    private void InvalidateCachedLists()
    {
        _cacheService.Remove(CacheKey.Departments);
    }
}