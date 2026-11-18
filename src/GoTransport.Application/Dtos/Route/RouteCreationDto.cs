namespace GoTransport.Application.Dtos.Route;

public class RouteCreationDto
{
    public string Description { get; set; } = null!;

    public int OriginPointId { get; set; }

    public int DestinationPointId { get; set; }
}