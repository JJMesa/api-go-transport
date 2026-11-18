using GoTransport.Application.Dtos.Schedule;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Mappings;

internal static class ScheduleMappingExtensions
{
    /// <summary>
    /// Maps a schedule to its DTO. A navigation property that was not loaded maps to null.
    /// </summary>
    public static ScheduleDto ToDto(this Schedule schedule) => new()
    {
        ScheduleId = schedule.ScheduleId,
        DepartureTime = schedule.DepartureTime,
        ArrivalTime = schedule.ArrivalTime,
        Duration = schedule.Duration,
        Vehicle = schedule.Vehicle?.ToDto(),
        Route = schedule.Route?.ToDto()
    };

    public static Schedule ToEntity(this ScheduleCreationDto scheduleCreation) => new()
    {
        DepartureTime = scheduleCreation.DepartureTime,
        ArrivalTime = scheduleCreation.ArrivalTime,
        VehicleId = scheduleCreation.VehicleId,
        RouteId = scheduleCreation.RouteId
    };

    public static void ApplyTo(this ScheduleUpdateDto scheduleUpdate, Schedule schedule)
    {
        schedule.DepartureTime = scheduleUpdate.DepartureTime;
        schedule.ArrivalTime = scheduleUpdate.ArrivalTime;
        schedule.VehicleId = scheduleUpdate.VehicleId;
        schedule.RouteId = scheduleUpdate.RouteId;
        schedule.IsActive = scheduleUpdate.IsActive;
    }
}
