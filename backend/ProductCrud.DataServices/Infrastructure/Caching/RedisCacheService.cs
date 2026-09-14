using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using ProductCrud.DataServices.Infrastructure.Caching;

namespace ProductCrud.Api.Infrastructure.Caching;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;

    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    public RedisCacheService(
        IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetAsync<T>(
        string key,
        CancellationToken cancellationToken = default)
    {
        var json = await _cache.GetStringAsync(
            key,
            cancellationToken);

        if (string.IsNullOrWhiteSpace(json))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(
            json,
            JsonOptions);
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(
            value,
            JsonOptions);

        var options =
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow =
                    expiration
            };

        await _cache.SetStringAsync(
            key,
            json,
            options,
            cancellationToken);
    }

    public Task RemoveAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        return _cache.RemoveAsync(
            key,
            cancellationToken);
    }
}