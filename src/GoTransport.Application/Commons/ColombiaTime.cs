namespace GoTransport.Application.Commons;

/// <summary>
/// Colombia's time zone (UTC−5, no daylight saving). Schedules and travel dates are Colombian wall-clock
/// times, so business rules compare them with Colombian "now". Stored timestamps are UTC.
/// </summary>
public static class ColombiaTime
{
    private static readonly TimeZoneInfo Zone = TimeZoneInfo.FindSystemTimeZoneById("America/Bogota");

    /// <summary>The current Colombian wall-clock time according to <paramref name="clock"/>.</summary>
    public static DateTime Now(TimeProvider clock) => TimeZoneInfo.ConvertTimeFromUtc(clock.GetUtcNow().UtcDateTime, Zone);
}
