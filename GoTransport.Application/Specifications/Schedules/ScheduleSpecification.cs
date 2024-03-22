using Ardalis.Specification;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Specifications.Schedules;

public class ScheduleSpecification : Specification<Schedule>
{
    public ScheduleSpecification(int ruoteId)
    {
        Query.Where(schedule => schedule.IsActive!.Value);
        Query.Where(Schedule => Schedule.RouteId == ruoteId);

        Query.OrderBy(schedule => schedule.DepartureTime);

        Query.Include(schedule => schedule.Route!.OriginPoint.City);
        Query.Include(schedule => schedule.Route!.DestinationPoint.City);
    }

    public ScheduleSpecification(Guid scheduleId)
    {
        Query.Where(schedule => schedule.ScheduleId.Equals(scheduleId));

        Query.Include(schedule => schedule.Vehicle.Manufacturer);
    }
}