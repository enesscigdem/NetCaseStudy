using System.Collections.Concurrent;
using System.Text.Json;
using NetCaseStudy.Application.Abstractions;

namespace NetCaseStudy.Tests.TestUtils;

public class FakeCacheService : ICacheService
{
    private readonly ConcurrentDictionary<string, string> _store = new();

    public Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan ttl)
    {
        if (_store.TryGetValue(key, out var json))
        {
            return Task.FromResult(JsonSerializer.Deserialize<T>(json));
        }

        return SetAsync();

        async Task<T?> SetAsync()
        {
            var value = await factory();
            _store[key] = JsonSerializer.Serialize(value);
            return value;
        }
    }

    public Task RemoveAsync(string key)
    {
        _store.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefix)
    {
        foreach (var key in _store.Keys.Where(k => k.StartsWith(prefix)))
            _store.TryRemove(key, out _);
        return Task.CompletedTask;
    }
}