using System.Text.Json;

namespace NetCaseStudy.Application.Abstractions;

public interface ICacheService
{
    Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan ttl);
    Task RemoveAsync(string key);

    Task RemoveByPrefixAsync(string prefix);
}