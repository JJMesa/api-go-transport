namespace GoTransport.Application.Dtos.City;

public class CityUpdateDto
{
    public int CityId { get; set; }

    public string Description { get; set; } = null!;

    public int DepartmentId { get; set; }

    public bool IsActive { get; set; }
}