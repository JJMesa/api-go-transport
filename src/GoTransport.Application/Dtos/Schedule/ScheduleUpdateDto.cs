namespace GoTransport.Application.Dtos.Schedule;

public class ScheduleUpdateDto
{
    public Guid ScheduleId { get; set; }

    public TimeSpan DepartureTime { get; set; }

    public TimeSpan ArrivalTime { get; set; }

    public int VehicleId { get; set; }

    public int RouteId { get; set; }

    public bool IsActive { get; set; }
}