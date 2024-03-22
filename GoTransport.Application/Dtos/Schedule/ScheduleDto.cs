using GoTransport.Application.Dtos.Route;
using GoTransport.Application.Dtos.Vehicle;

namespace GoTransport.Application.Dtos.Schedule;

public class ScheduleDto
{
    public Guid ScheduleId { get; set; }

    public TimeSpan DepartureTime { get; set; }

    public TimeSpan ArrivalTime { get; set; }

    public TimeSpan Duration { get; set; }

    public VehicleDto? Vehicle { get; set; }

    public RouteDto? Route { get; set; }
}