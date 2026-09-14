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

    public Task<T?> GetAsync<T>(
        string key,
        CancellationToken cancellationToken = default)
    {
        _memoryCache.TryGetValue(
            key,
            out T? value);

        return Task.FromResult(value);
    }

    public Task SetAsync<T>(
        string key,
        T value,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        _memoryCache.Set(
            key,
            value,
            expiration);

        return Task.CompletedTask;
    }

    public Task RemoveAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        _memoryCache.Remove(key);

        return Task.CompletedTask;
    }
}