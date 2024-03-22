using AutoMapper;
using GoTransport.Application.Attributes;
using GoTransport.Application.Builders;
using GoTransport.Application.Commons;
using GoTransport.Application.Dtos.City;
using GoTransport.Application.Interfaces;
using GoTransport.Application.Interfaces.Base;
using GoTransport.Application.Parameters;
using GoTransport.Application.Specifications.Cities;
using GoTransport.Application.Wrappers;
using GoTransport.Domain.Entities.Bas;

namespace GoTransport.Application.Services;

[Transient]
public class CityService : ICityService
{
    private readonly IRepository<City> _cityRepository;
    private readonly IMapper _mapper;

    public CityService(IRepository<City> cityRepository
        , IMapper mapper)
    {
        _cityRepository = cityRepository;
        _mapper = mapper;
    }

    public async Task<JsonResponse<IEnumerable<CityDto>>> GetAllByDepartmentAsync(int departmentId, CancellationToken cancellationToken)
    {
        var cities = await _cityRepository.ListAsync(new CitySpecification(departmentId: departmentId), cancellationToken);
        return ResponseBuilder<IEnumerable<CityDto>>.Ok(_mapper.Map<IEnumerable<CityDto>>(cities));
    }

    public async Task<JsonPagedResponse<IEnumerable<CityDto>>> GetAsync(CityParameters parameters, CancellationToken cancellationToken)
    {
        var cities = await _cityRepository.ListAsync(new PagedCitySpecification(parameters), cancellationToken);
        var totalRecords = await _cityRepository.CountAsync(new CitySpecification(parameters), cancellationToken);
        var metadata = Metadata.Create(parameters.PageNumber, parameters.PageSize, totalRecords);
        return ResponseBuilder<IEnumerable<CityDto>>.OkPaged(cities, metadata);
    }

    public async Task<JsonResponse<CityDto>> GetByIdAsync(dynamic id, CancellationToken cancellationToken)
    {
        var city = await _cityRepository.FirstOrDefaultAsync(new CitySpecification(id, null), cancellationToken);
        if (city is null) return ResponseBuilder<CityDto>.NotFound();
        return ResponseBuilder<CityDto>.Ok(_mapper.Map<CityDto>(city));
    }

    public async Task<JsonResponse<CityDto>> CreateAsync(CityCreationDto cityCreation)
    {
        if (await IsDuplicateDescriptionAsync(cityCreation.Description, cityCreation.DepartmentId))
            return ResponseBuilder<CityDto>.BadRequest(ErrorMessages.DuplicateDescription);

        var city = _mapper.Map<City>(cityCreation);
        await _cityRepository.AddAsync(city);

        return ResponseBuilder<CityDto>.Created(_mapper.Map<CityDto>(city));
    }

    public async Task<JsonResponse<CityDto>> UpdateAsync(dynamic id, CityUpdateDto cityUpdateDto)
    {
        if (id != cityUpdateDto.CityId)
            return ResponseBuilder<CityDto>.BadRequest(ErrorMessages.UrlAndBodyIdNotEqual);

        var city = await _cityRepository.GetByIdAsync(id);
        if (city is null) return ResponseBuilder<CityDto>.NotFound();

        if (await IsDuplicateDescriptionAsync(cityUpdateDto.Description, cityUpdateDto.DepartmentId, id))
            return ResponseBuilder<CityDto>.BadRequest(ErrorMessages.DuplicateDescription);

        _mapper.Map(cityUpdateDto, city);
        await _cityRepository.UpdateAsync(city);
        return ResponseBuilder<CityDto>.Ok(_mapper.Map<CityDto>(city));
    }

    public async Task<JsonResponse<bool?>> DeleteAsync(dynamic id)
    {
        var city = await _cityRepository.GetByIdAsync(id);
        if (city is null) return ResponseBuilder<bool?>.NotFound();
        await _cityRepository.DeleteAsync(city);
        return ResponseBuilder<bool?>.NoContent();
    }

    private async Task<bool> IsDuplicateDescriptionAsync(string description, int departmentId, int id = 0) =>
        await _cityRepository.AnyAsync(new DuplicationCitySpecification(description, departmentId, id));
}