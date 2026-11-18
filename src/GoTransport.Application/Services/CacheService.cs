using GoTransport.Application.Interfaces;
using GoTransport.Application.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace GoTransport.Application.Services;

internal class CacheService<T> : ICacheService<T>
{
    private readonly IMemoryCache _memoryCache;
    private readonly CachingSettings _cachingSettings;

    public CacheService(IMemoryCache memoryCache
        , IOptionsSnapshot<CachingSettings> cachingSettings)
    {
        _memoryCache = memoryCache;
        _cachingSettings = cachingSettings.Value;
    }

    public List<T>? Get(string cacheKey)
    {
        _memoryCache.TryGetValue(cacheKey, out List<T>? data);
        return data;
    }

    public void Set(string cacheKey, List<T> data)
    {
        var slidingByTimespan = _cachingSettings.SlidingExpiration;
        var absoluteByTimespan = _cachingSettings.AbsoluteExpiration;

        MemoryCacheEntryOptions cacheOptions = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(absoluteByTimespan),
            SlidingExpiration = TimeSpan.FromMinutes(slidingByTimespan)
        };

        _memoryCache.Set(cacheKey, data, cacheOptions);
    }

    public void Remove(string cacheKey)
    {
        _memoryCache.Remove(cacheKey);
    }
}