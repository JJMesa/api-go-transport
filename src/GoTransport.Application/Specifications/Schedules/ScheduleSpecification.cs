using Ardalis.Specification;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Specifications.Schedules;

public class ScheduleSpecification : Specification<Schedule>
{
    public ScheduleSpecification(int routeId)
    {
        Query.Where(schedule => schedule.IsActive);
        Query.Where(schedule => schedule.RouteId == routeId);

        Query.OrderBy(schedule => schedule.DepartureTime);

        Query.Include(schedule => schedule.Route!.OriginPoint.City);
        Query.Include(schedule => schedule.Route!.DestinationPoint.City);

        Query.Include(schedule => schedule.Vehicle.Manufacturer);

        // Read-only: nothing loaded here is saved back, so skip change tracking.
        Query.AsNoTracking();
    }

    /// <summary>
    /// One Schedule with its Vehicle and Route, as the detail response and the Reservation rules need them.
    /// </summary>
    public ScheduleSpecification(Guid scheduleId)
    {
        Query.Where(schedule => schedule.ScheduleId.Equals(scheduleId));

        Query.Include(schedule => schedule.Vehicle.Manufacturer);
        Query.Include(schedule => schedule.Route.OriginPoint.City);
        Query.Include(schedule => schedule.Route.DestinationPoint.City);
    }
}