using GoTransport.Application.Dtos.City;

namespace GoTransport.Application.Dtos.Point;

public class PointDto
{
    public int PointId { get; set; }

    public CityDto? City { get; set; }

    public string Detail { get; set; } = null!;
}