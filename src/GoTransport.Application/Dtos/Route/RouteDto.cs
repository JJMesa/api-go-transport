using GoTransport.Application.Dtos.Point;

namespace GoTransport.Application.Dtos.Route;

public class RouteDto
{
    public int RouteId { get; set; }

    public string Description { get; set; } = null!;

    public PointDto? OriginPoint { get; set; }

    public PointDto? DestinationPoint { get; set; }

    public bool IsActive { get; set; }
}