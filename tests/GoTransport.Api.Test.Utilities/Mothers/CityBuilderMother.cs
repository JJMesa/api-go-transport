using GoTransport.Api.Test.Utilities.Commons;
using GoTransport.Application.Dtos.City;
using GoTransport.Application.Enums;
using GoTransport.Application.Parameters;

namespace GoTransport.Api.Test.Utilities.Mothers;

public static class CityBuilderMother
{
    public static CityParameters CityParameters(int? departmentId = null, int pageNumber = 1, int pageSize = 5, string? search = null, SortDirection? sort = null)
    {
        return new CityParameters
        {
            DepartmentId = departmentId,
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchCriteria = search,
            OrderBy = sort
        };
    }

    public static CityCreationDto CityCreationDtoOk(int departmentId = Utils.DefaultId)
    {
        return new CityCreationDto
        {
            Description = "Prueba Creación",
            DepartmentId = departmentId
        };
    }

    public static CityUpdateDto CityUpdateDtoOk(int cityId = Utils.DefaultId, int departmentId = Utils.DefaultId)
    {
        return new CityUpdateDto
        {
            CityId = cityId,
            Description = "Prueba Actualización",
            DepartmentId = departmentId,
            IsActive = true
        };
    }
}