using System;
using System.Threading;
using System.Threading.Tasks;
using AcadPrep.Application.Common.Models;
using Application.Common.Interfaces;
using Application.Common.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public sealed class CacheQuotaService : IQuotaService
{
    private readonly ICacheService _cacheService;
    private readonly FreemiumSettings _freemiumSettings;
    private readonly TimeProvider _timeProvider;
    private readonly IMemoryCache _lockCache;

    public CacheQuotaService(
        ICacheService cacheService, 
        IOptions<FreemiumSettings> freemiumOptions, 
        TimeProvider timeProvider,
        IMemoryCache lockCache)
    {
        _cacheService = cacheService;
        _freemiumSettings = freemiumOptions.Value;
        _timeProvider = timeProvider;
        _lockCache = lockCache;
    }

    public async Task<Result<int>> CheckAndConsumeAsync(int userId, bool isPro, int tokenAmount, CancellationToken cancellationToken = default)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        // Using Vietnam time (UTC+7) for quota reset
        var localTime = now.AddHours(7); 
        var cacheKey = $"quota:ai-qna:{userId}:{localTime:yyyy-MM-dd}";
        
        int limit = isPro ? _freemiumSettings.AiQnaTokensPerDayPro : _freemiumSettings.AiQnaTokensPerDayFree;

        // Get or create SemaphoreSlim for this specific key
        // SlidingExpiration ensures the lock is removed from memory after 10 minutes of inactivity
        var semaphore = _lockCache.GetOrCreate(cacheKey + ":lock", entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(10);
            return new SemaphoreSlim(1, 1);
        });

        if (semaphore == null)
        {
            return Result<int>.Failure("Không thể lấy khóa đồng bộ. Vui lòng thử lại.");
        }

        await semaphore.WaitAsync(cancellationToken);
        try
        {
            int currentUsage = await _cacheService.GetAsync<int>(cacheKey, cancellationToken);
            
            if (currentUsage + tokenAmount > limit)
            {
                return Result<int>.Failure($"Vượt quá giới hạn {limit} tokens hôm nay.");
            }

            int newUsage = currentUsage + tokenAmount;
            
            // Calculate time until midnight local time for cache expiration
            var midnightLocal = localTime.Date.AddDays(1);
            var timeUntilMidnight = midnightLocal - localTime;
            
            await _cacheService.SetAsync(cacheKey, newUsage, timeUntilMidnight, cancellationToken);
            
            return Result<int>.Success(limit - newUsage);
        }
        finally
        {
            semaphore.Release();
        }
    }
}
