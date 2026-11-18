namespace GoTransport.Application.Extensions;

public static class DateTimeExtensions
{
    public static DateTime ToPacificStandardTime(this DateTime dateTime)
    {
        TimeZoneInfo timeZoneColombia = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
        return TimeZoneInfo.ConvertTimeFromUtc(dateTime, timeZoneColombia);
    }
}