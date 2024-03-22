namespace GoTransport.Application.Dtos.Schedule;

public class ScheduleCreationDto
{
    public TimeSpan DepartureTime { get; set; }

    public TimeSpan ArrivalTime { get; set; }

    public int VehicleId { get; set; }

    public int RouteId { get; set; }
}