namespace Application.Common.Interfaces;

public sealed class SubscriptionSnapshot
{
    public bool IsPro { get; init; }
    public DateTime? StartsAt { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public string? PlanCode { get; init; }
    public string? PlanName { get; init; }
    public int RemainingDays { get; init; }
}

public sealed class QuotaCheckResult
{
    public bool Allowed { get; init; }
    public bool IsPro { get; init; }
    public string? ErrorCode { get; init; }
    public string? Message { get; init; }
    public int Used { get; init; }
    public int Limit { get; init; }

    public static QuotaCheckResult Allow(bool isPro, int used = 0, int limit = int.MaxValue) => new()
    {
        Allowed = true,
        IsPro = isPro,
        Used = used,
        Limit = limit
    };

    public static QuotaCheckResult Deny(string errorCode, string message, int used, int limit, bool isPro = false) => new()
    {
        Allowed = false,
        IsPro = isPro,
        ErrorCode = errorCode,
        Message = message,
        Used = used,
        Limit = limit
    };
}

/// <summary>Kiểm tra Pro entitlement và hạn mức Free.</summary>
public interface IBillingAccessService
{
    Task<SubscriptionSnapshot> GetSubscriptionAsync(int userId, CancellationToken cancellationToken = default);
    Task<bool> IsProAsync(int userId, CancellationToken cancellationToken = default);
    Task<QuotaCheckResult> EnsureCanStartFullTestAsync(int userId, CancellationToken cancellationToken = default);
    Task<QuotaCheckResult> EnsureCanStartPracticeAsync(int userId, CancellationToken cancellationToken = default);
    Task<QuotaCheckResult> EnsureCanSaveVocabularyAsync(int userId, CancellationToken cancellationToken = default);
}
