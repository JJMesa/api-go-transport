using GoTransport.Application.Enums;
using GoTransport.Application.Parameters;

namespace GoTransport.Api.Unit.Tests.Mothers;

internal static class CityBuilderMother
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
}
