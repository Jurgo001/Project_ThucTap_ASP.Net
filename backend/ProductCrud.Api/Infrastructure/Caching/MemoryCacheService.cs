using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using ProductCrud.DataServices.Infrastructure.Caching;

namespace ProductCrud.Api.Infrastructure.Caching;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;

    public MemoryCacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public bool TryGetValue<T>(
        string key,
        out T? value)
    {
        return _memoryCache.TryGetValue(
            key,
            out value);
    }

    public void Set<T>(
        string key,
        T value,
        TimeSpan expiration)
    {
        _memoryCache.Set(
            key,
            value,
            expiration);
    }

    public void Remove(string key)
    {
        _memoryCache.Remove(key);
    }
}