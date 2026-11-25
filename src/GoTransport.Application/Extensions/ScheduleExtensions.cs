using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Extensions;

/// <summary>
/// Time rules of a Schedule. Departure and arrival are times of day, so an arrival at or before the
/// departure time falls on the next day (an overnight Schedule).
/// </summary>
public static class ScheduleExtensions
{
    private static readonly TimeSpan OneDay = TimeSpan.FromDays(1);

    /// <summary>
    /// How long one Trip takes, from departure to arrival.
    /// </summary>
    public static TimeSpan TripDuration(TimeSpan departureTime, TimeSpan arrivalTime) =>
        arrivalTime > departureTime ? arrivalTime - departureTime : arrivalTime - departureTime + OneDay;

    /// <summary>
    /// When the Trip of this Schedule on <paramref name="travelDate"/> arrives, which for an overnight
    /// Schedule is on the following day.
    /// </summary>
    public static DateTime ArrivalOn(this Schedule schedule, DateTime travelDate) =>
        travelDate.Date + schedule.DepartureTime + TripDuration(schedule.DepartureTime, schedule.ArrivalTime);
}
