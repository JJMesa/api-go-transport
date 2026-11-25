using System.ComponentModel.DataAnnotations;

namespace GoTransport.Application.Settings;

/// <summary>
/// Cache lifetimes, in minutes.
/// </summary>
public class CachingSettings
{
    [Range(1, int.MaxValue)]
    public int SlidingExpiration { get; set; }

    [Range(1, int.MaxValue)]
    public int AbsoluteExpiration { get; set; }
}