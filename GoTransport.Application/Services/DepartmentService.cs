using AutoMapper;
using GoTransport.Application.Attributes;
using GoTransport.Application.Builders;
using GoTransport.Application.Commons;
using GoTransport.Application.Dtos.Department;
using GoTransport.Application.Interfaces;
using GoTransport.Application.Interfaces.Base;
using GoTransport.Application.Parameters;
using GoTransport.Application.Specifications.Departments;
using GoTransport.Application.Wrappers;
using GoTransport.Domain.Entities.Bas;

namespace GoTransport.Application.Services;

[Transient]
internal class DepartmentService : IDepartmentService
{
    private readonly IRepository<Department> _departmentRepository;
    private readonly IMapper _mapper;
    private readonly ICacheService<Department> _cacheService;

    public DepartmentService(IRepository<Department> departmentRepository
        , IMapper mapper
        , ICacheService<Department> cacheService)
    {
        _departmentRepository = departmentRepository;
        _mapper = mapper;
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

        return ResponseBuilder<IEnumerable<DepartmentDto>>.Ok(_mapper.Map<IEnumerable<DepartmentDto>>(departments));
    }

    public async Task<JsonPagedResponse<IEnumerable<DepartmentDto>>> GetAsync(DepartmentParameters parameters, CancellationToken cancellationToken)
    {
        var departments = await _departmentRepository.ListAsync(new PagedDepartmentSpecification(parameters), cancellationToken);
        var totalAmount = await _departmentRepository.CountAsync(new DepartmentSpecification(parameters), cancellationToken);
        var metadata = Metadata.Create(parameters.PageNumber, parameters.PageSize, totalAmount);
        return ResponseBuilder<IEnumerable<DepartmentDto>>.OkPaged(departments, metadata);
    }

    public async Task<JsonResponse<DepartmentDto>> GetByIdAsync(dynamic departmentId, CancellationToken cancellationToken)
    {
        var department = await _departmentRepository.GetByIdAsync(departmentId, cancellationToken);
        if (department is null) return ResponseBuilder<DepartmentDto>.NotFound();
        return ResponseBuilder<DepartmentDto>.Ok(_mapper.Map<DepartmentDto>(department));
    }

    public async Task<JsonResponse<DepartmentDto>> CreateAsync(DepartmentCreationDto departmentCreation)
    {
        if (await IsDuplicateDescriptionAsync(departmentCreation.Description))
            return ResponseBuilder<DepartmentDto>.BadRequest(ErrorMessages.DuplicateDescription);

        var department = _mapper.Map<Department>(departmentCreation);
        await _departmentRepository.AddAsync(department);

        return ResponseBuilder<DepartmentDto>.Created(_mapper.Map<DepartmentDto>(department));
    }

    public async Task<JsonResponse<DepartmentDto>> UpdateAsync(dynamic id, DepartmentUpdateDto departmentUpdate)
    {
        if (id != departmentUpdate.DepartmentId)
            return ResponseBuilder<DepartmentDto>.BadRequest(ErrorMessages.UrlAndBodyIdNotEqual);

        var department = await _departmentRepository.GetByIdAsync(id);
        if (department is null) return ResponseBuilder<DepartmentDto>.NotFound();

        if (await IsDuplicateDescriptionAsync(departmentUpdate.Description, id))
            return ResponseBuilder<DepartmentDto>.BadRequest(ErrorMessages.DuplicateDescription);

        _mapper.Map(departmentUpdate, department);
        await _departmentRepository.UpdateAsync(department);
        return ResponseBuilder<DepartmentDto>.Ok(_mapper.Map<DepartmentDto>(department));
    }

    public async Task<JsonResponse<bool?>> DeleteAsync(dynamic departmentId)
    {
        var department = await _departmentRepository.GetByIdAsync(departmentId);
        if (department is null) return ResponseBuilder<bool?>.NotFound();
        await _departmentRepository.DeleteAsync(department);
        return ResponseBuilder<bool?>.NoContent();
    }

    private async Task<bool> IsDuplicateDescriptionAsync(string description, int id = 0) =>
        await _departmentRepository.AnyAsync(new DuplicationDepartmentSpecification(description, id));
}