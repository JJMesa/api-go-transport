namespace GoTransport.Application.Settings;

public class CachingSettings
{
    public int SlidingExpiration { get; set; }
    public int AbsoluteExpiration { get; set; }
}