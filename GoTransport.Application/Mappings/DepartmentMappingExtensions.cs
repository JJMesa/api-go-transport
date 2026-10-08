using GoTransport.Application.Dtos.Department;
using GoTransport.Domain.Entities.Bas;

namespace GoTransport.Application.Mappings;

internal static class DepartmentMappingExtensions
{
    public static DepartmentDto ToDto(this Department department) => new()
    {
        DepartmentId = department.DepartmentId,
        Description = department.Description,
        IsActive = department.IsActive ?? false
    };

    public static Department ToEntity(this DepartmentCreationDto departmentCreation) => new()
    {
        Description = departmentCreation.Description
    };

    public static void ApplyTo(this DepartmentUpdateDto departmentUpdate, Department department)
    {
        department.Description = departmentUpdate.Description;
        department.IsActive = departmentUpdate.IsActive;
    }
}
