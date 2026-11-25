using System.ComponentModel.DataAnnotations;

namespace GoTransport.Application.Settings;

public class LockoutSettings
{
    [Range(1, 100)]
    public int MaxFailedAccessAttempts { get; set; } = 5;

    [Range(1, 1440)]
    public int LockoutTimeInMinutes { get; set; } = 15;
}
