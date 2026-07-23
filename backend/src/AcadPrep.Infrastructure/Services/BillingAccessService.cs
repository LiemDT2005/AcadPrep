using Application.Common.Interfaces;
using Application.Common.Options;
using Domain.Constants;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public sealed class BillingAccessService : IBillingAccessService
{
    private readonly IAppDbContext _context;
    private readonly FreemiumSettings _freemium;
    private readonly TimeProvider _timeProvider;

    public BillingAccessService(
        IAppDbContext context,
        IOptions<FreemiumSettings> freemium,
        TimeProvider timeProvider)
    {
        _context = context;
        _freemium = freemium.Value;
        _timeProvider = timeProvider;
    }

    public async Task<SubscriptionSnapshot> GetSubscriptionAsync(int userId, CancellationToken cancellationToken = default)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;

        var sub = await _context.UserSubscriptions
            .AsNoTracking()
            .Include(s => s.Plan)
            .Where(s => s.UserId == userId && s.Status == SubscriptionStatus.Active && s.ExpiresAt > now)
            .OrderByDescending(s => s.ExpiresAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (sub is null)
        {
            return new SubscriptionSnapshot { IsPro = false };
        }

        var remaining = (int)Math.Ceiling((sub.ExpiresAt - now).TotalDays);
        if (remaining < 0) remaining = 0;

        return new SubscriptionSnapshot
        {
            IsPro = true,
            StartsAt = sub.StartsAt,
            ExpiresAt = sub.ExpiresAt,
            PlanCode = sub.Plan.Code,
            PlanName = sub.Plan.Name,
            RemainingDays = remaining
        };
    }

    public async Task<bool> IsProAsync(int userId, CancellationToken cancellationToken = default)
    {
        var snapshot = await GetSubscriptionAsync(userId, cancellationToken);
        return snapshot.IsPro;
    }

    public async Task<QuotaCheckResult> EnsureCanStartFullTestAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (await IsProAsync(userId, cancellationToken))
        {
            return QuotaCheckResult.Allow(isPro: true);
        }

        var (monthStart, monthEnd) = GetUtcMonthBoundsVietnam();
        var used = await _context.ExamAttempts
            .AsNoTracking()
            .CountAsync(a =>
                a.UserId == userId &&
                a.StartedAt >= monthStart &&
                a.StartedAt < monthEnd,
                cancellationToken);

        var limit = Math.Max(0, _freemium.FullTestsPerMonth);
        if (used >= limit)
        {
            return QuotaCheckResult.Deny(
                BillingCodes.ProRequired(BillingCodes.FullTestQuota),
                $"You have used all {limit} free Full Test(s) this month. Upgrade to Pro for unlimited access.",
                used,
                limit);
        }

        return QuotaCheckResult.Allow(isPro: false, used, limit);
    }

    public async Task<QuotaCheckResult> EnsureCanStartPracticeAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (await IsProAsync(userId, cancellationToken))
        {
            return QuotaCheckResult.Allow(isPro: true);
        }

        var (dayStart, dayEnd) = GetUtcDayBoundsVietnam();
        var used = await _context.PracticeSessions
            .AsNoTracking()
            .CountAsync(s =>
                s.UserId == userId &&
                s.CreatedAt >= dayStart &&
                s.CreatedAt < dayEnd,
                cancellationToken);

        var limit = Math.Max(0, _freemium.PracticeSessionsPerDay);
        if (used >= limit)
        {
            return QuotaCheckResult.Deny(
                BillingCodes.ProRequired(BillingCodes.PracticeQuota),
                $"You have used all {limit} free Practice session(s) today. Upgrade to Pro for unlimited practice.",
                used,
                limit);
        }

        return QuotaCheckResult.Allow(isPro: false, used, limit);
    }

    public async Task<QuotaCheckResult> EnsureCanSaveVocabularyAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (await IsProAsync(userId, cancellationToken))
        {
            return QuotaCheckResult.Allow(isPro: true);
        }

        var used = await _context.SavedVocabularies
            .AsNoTracking()
            .CountAsync(sv => sv.UserId == userId, cancellationToken);

        var limit = Math.Max(0, _freemium.SavedVocabularyMax);
        if (used >= limit)
        {
            return QuotaCheckResult.Deny(
                BillingCodes.ProRequired(BillingCodes.VocabQuota),
                $"Free notebook is limited to {limit} words. Upgrade to Pro to save without limits.",
                used,
                limit);
        }

        return QuotaCheckResult.Allow(isPro: false, used, limit);
    }

    /// <summary>Ranh giới tháng theo lịch Việt Nam, trả về UTC.</summary>
    private (DateTime StartUtc, DateTime EndUtc) GetUtcMonthBoundsVietnam()
    {
        var tz = GetVietnamTimeZone();
        var nowVn = TimeZoneInfo.ConvertTimeFromUtc(_timeProvider.GetUtcNow().UtcDateTime, tz);
        var startVn = new DateTime(nowVn.Year, nowVn.Month, 1, 0, 0, 0, DateTimeKind.Unspecified);
        var endVn = startVn.AddMonths(1);
        return (TimeZoneInfo.ConvertTimeToUtc(startVn, tz), TimeZoneInfo.ConvertTimeToUtc(endVn, tz));
    }

    private (DateTime StartUtc, DateTime EndUtc) GetUtcDayBoundsVietnam()
    {
        var tz = GetVietnamTimeZone();
        var nowVn = TimeZoneInfo.ConvertTimeFromUtc(_timeProvider.GetUtcNow().UtcDateTime, tz);
        var startVn = nowVn.Date;
        var endVn = startVn.AddDays(1);
        return (TimeZoneInfo.ConvertTimeToUtc(startVn, tz), TimeZoneInfo.ConvertTimeToUtc(endVn, tz));
    }

    private static TimeZoneInfo GetVietnamTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
        }
    }
}
