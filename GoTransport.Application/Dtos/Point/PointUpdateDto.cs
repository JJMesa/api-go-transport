namespace GoTransport.Application.Dtos.Point;

public class PointUpdateDto
{
    public int PointId { get; set; }

    public int CityId { get; set; }

    public string Detail { get; set; } = null!;

    public bool IsActive { get; set; }
}