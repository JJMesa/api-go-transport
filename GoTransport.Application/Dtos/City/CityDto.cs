using GoTransport.Application.Dtos.Department;

namespace GoTransport.Application.Dtos.City;

public class CityDto
{
    public int CityId { get; set; }

    public string Description { get; set; } = null!;

    public DepartmentDto Department { get; set; } = null!;

    public bool IsActive { get; set; }
}