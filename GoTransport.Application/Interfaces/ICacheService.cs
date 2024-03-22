namespace GoTransport.Application.Interfaces;

public interface ICacheService<T>
{
    List<T>? Get(string cacheKey);

    void Set(string cacheKey, List<T> data);

    void Remove(string cacheKey);
}