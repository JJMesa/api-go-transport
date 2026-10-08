using GoTransport.Application.Attributes;
using GoTransport.Application.Builders;
using GoTransport.Application.Commons;
using GoTransport.Application.Dtos.City;
using GoTransport.Application.Interfaces;
using GoTransport.Application.Interfaces.Base;
using GoTransport.Application.Mappings;
using GoTransport.Application.Parameters;
using GoTransport.Application.Specifications.Cities;
using GoTransport.Application.Wrappers;
using GoTransport.Domain.Entities.Bas;

namespace GoTransport.Application.Services;

[Transient]
public class CityService : ICityService
{
    private readonly IRepository<City> _cityRepository;

    public CityService(IRepository<City> cityRepository)
    {
        _cityRepository = cityRepository;
    }

    public async Task<JsonResponse<IEnumerable<CityDto>>> GetAllByDepartmentAsync(int departmentId, CancellationToken cancellationToken)
    {
        var cities = await _cityRepository.ListAsync(new CitySpecification(departmentId: departmentId), cancellationToken);
        return ResponseBuilder<IEnumerable<CityDto>>.Ok(cities.Select(city => city.ToDto()).ToList());
    }

    public async Task<JsonPagedResponse<IEnumerable<CityDto>>> GetAsync(CityParameters parameters, CancellationToken cancellationToken)
    {
        var cities = await _cityRepository.ListAsync(new PagedCitySpecification(parameters), cancellationToken);
        var totalRecords = await _cityRepository.CountAsync(new CitySpecification(parameters), cancellationToken);
        var metadata = Metadata.Create(parameters.PageNumber, parameters.PageSize, totalRecords);
        return ResponseBuilder<IEnumerable<CityDto>>.OkPaged(cities, metadata);
    }

    public async Task<JsonResponse<CityDto>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var city = await _cityRepository.FirstOrDefaultAsync(new CitySpecification(id, null), cancellationToken);
        if (city is null) return ResponseBuilder<CityDto>.NotFound();
        return ResponseBuilder<CityDto>.Ok(city.ToDto());
    }

    public async Task<JsonResponse<CityDto>> CreateAsync(CityCreationDto cityCreation)
    {
        if (await IsDuplicateDescriptionAsync(cityCreation.Description, cityCreation.DepartmentId))
            return ResponseBuilder<CityDto>.BadRequest(ErrorMessages.DuplicateDescription);

        var city = cityCreation.ToEntity();
        await _cityRepository.AddAsync(city);

        return ResponseBuilder<CityDto>.Created(city.ToDto());
    }

    public async Task<JsonResponse<CityDto>> UpdateAsync(int id, CityUpdateDto cityUpdateDto)
    {
        if (id != cityUpdateDto.CityId)
            return ResponseBuilder<CityDto>.BadRequest(ErrorMessages.UrlAndBodyIdNotEqual);

        var city = await _cityRepository.GetByIdAsync(id);
        if (city is null) return ResponseBuilder<CityDto>.NotFound();

        if (await IsDuplicateDescriptionAsync(cityUpdateDto.Description, cityUpdateDto.DepartmentId, id))
            return ResponseBuilder<CityDto>.BadRequest(ErrorMessages.DuplicateDescription);

        cityUpdateDto.ApplyTo(city);
        await _cityRepository.UpdateAsync(city);
        return ResponseBuilder<CityDto>.Ok(city.ToDto());
    }

    public async Task<JsonResponse<bool?>> DeleteAsync(int id)
    {
        var city = await _cityRepository.GetByIdAsync(id);
        if (city is null) return ResponseBuilder<bool?>.NotFound();
        await _cityRepository.DeleteAsync(city);
        return ResponseBuilder<bool?>.NoContent();
    }

    private async Task<bool> IsDuplicateDescriptionAsync(string description, int departmentId, int id = 0) =>
        await _cityRepository.AnyAsync(new DuplicationCitySpecification(description, departmentId, id));
}