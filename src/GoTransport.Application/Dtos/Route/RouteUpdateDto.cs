namespace GoTransport.Application.Dtos.Route;

public class RouteUpdateDto
{
    public int RouteId { get; set; }

    public string Description { get; set; } = null!;

    public int OriginPointId { get; set; }

    public int DestinationPointId { get; set; }

    public bool IsActive { get; set; }
}