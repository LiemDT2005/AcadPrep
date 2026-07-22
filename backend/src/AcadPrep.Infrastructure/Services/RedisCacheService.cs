using System.Text.Json;
using Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace Infrastructure.Services;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;

    public RedisCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var cachedValue = await _cache.GetStringAsync(key, cancellationToken);
            if (string.IsNullOrEmpty(cachedValue))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(cachedValue);
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"[Cache Warning] Redis is offline or failed: {ex.Message}");
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new DistributedCacheEntryOptions();
            if (slidingExpiration.HasValue)
            {
                options.SetSlidingExpiration(slidingExpiration.Value);
            }
            else
            {
                // Mặc định cache 1 giờ nếu không truyền
                options.SetAbsoluteExpiration(TimeSpan.FromHours(1));
            }

            var serializedValue = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, serializedValue, options, cancellationToken);
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"[Cache Warning] Redis set failed: {ex.Message}");
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cache.RemoveAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"[Cache Warning] Redis remove failed: {ex.Message}");
        }
    }

    public async Task<long> GetVersionAsync(string versionKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var value = await _cache.GetStringAsync(versionKey, cancellationToken);
            if (!string.IsNullOrEmpty(value) && long.TryParse(value, out var version))
            {
                return version;
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"[Cache Warning] Redis get version failed: {ex.Message}");
        }

        return 0;
    }

    public async Task BumpVersionAsync(string versionKey, CancellationToken cancellationToken = default)
    {
        try
        {
            // Dùng tick UTC để đảm bảo giá trị tăng dần và không cần thao tác tăng nguyên tử.
            var newVersion = DateTime.UtcNow.Ticks;
            await _cache.SetStringAsync(versionKey, newVersion.ToString(), cancellationToken);
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"[Cache Warning] Redis bump version failed: {ex.Message}");
        }
    }
}
