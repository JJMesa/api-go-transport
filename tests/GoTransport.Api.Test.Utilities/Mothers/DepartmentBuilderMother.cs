using GoTransport.Api.Test.Utilities.Commons;
using GoTransport.Application.Dtos.Department;
using GoTransport.Application.Enums;
using GoTransport.Application.Parameters;

namespace GoTransport.Api.Test.Utilities.Mothers;

public static class DepartmentBuilderMother
{
    public static DepartmentParameters DepartmentParameters(int pageNumber = 1, int pageSize = 5, string? search = null, SortDirection? sort = null, bool? isActive = null)
    {
        return new DepartmentParameters
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchCriteria = search,
            OrderBy = sort,
            IsActive = isActive
        };
    }

    public static DepartmentCreationDto DepartmentCreationDtoOk()
    {
        return new DepartmentCreationDto
        {
            Description = "Prueba Creación"
        };
    }

    public static DepartmentUpdateDto DepartmentUpdateDtoOk(int departmentId = Utils.DefaultId)
    {
        return new DepartmentUpdateDto
        {
            DepartmentId = departmentId,
            Description = "Prueba Actualización",
            IsActive = true
        };
    }
}
