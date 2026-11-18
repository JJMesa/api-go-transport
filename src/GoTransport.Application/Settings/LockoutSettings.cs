namespace GoTransport.Application.Settings;

public class LockoutSettings
{
    public int MaxFailedAccessAttempts { get; set; } = 5;
    public int LockoutTimeInMinutes { get; set; } = 15;
}
