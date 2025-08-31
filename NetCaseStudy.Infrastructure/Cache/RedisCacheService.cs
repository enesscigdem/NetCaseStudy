using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using NetCaseStudy.Application.Abstractions;
using StackExchange.Redis;

namespace NetCaseStudy.Infrastructure.Cache;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly IConnectionMultiplexer _mux;

    public RedisCacheService(IDistributedCache cache, IConnectionMultiplexer mux)
    {
        _cache = cache;
        _mux = mux;
    }

    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan ttl)
    {
        var cachedJson = await _cache.GetStringAsync(key);
        if (cachedJson is not null)
            return JsonSerializer.Deserialize<T>(cachedJson);

        var value = await factory();
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        };
        await _cache.SetStringAsync(key, JsonSerializer.Serialize(value), options);
        return value;
    }

    public Task RemoveAsync(string key) => _cache.RemoveAsync(key);

    public async Task RemoveByPrefixAsync(string prefix)
    {
        var endpoints = _mux.GetEndPoints();
        foreach (var ep in endpoints)
        {
            var server = _mux.GetServer(ep);
            foreach (var key in server.Keys(pattern: $"{prefix}*"))
            {
                await _cache.RemoveAsync(key);
            }
        }
    }
}