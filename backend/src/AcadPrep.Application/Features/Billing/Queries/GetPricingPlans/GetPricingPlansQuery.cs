using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.Billing.Queries.GetPricingPlans;

public record GetPricingPlansQuery(int? UserId) : IRequest<Result<PricingPageDto>>;

public sealed class PricingPageDto
{
    public IReadOnlyList<PlanDto> Plans { get; init; } = Array.Empty<PlanDto>();
    public SubscriptionStatusDto? Subscription { get; init; }
    public FreemiumQuotaDto FreeQuota { get; init; } = new();
    public IReadOnlyList<MyOrderDto> RecentOrders { get; init; } = Array.Empty<MyOrderDto>();
}

public sealed class PlanDto
{
    public int Id { get; init; }
    public string Code { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public long PriceVnd { get; init; }
    public int DurationDays { get; init; }
    public bool IsHighlighted { get; init; }
}

public sealed class SubscriptionStatusDto
{
    public bool IsPro { get; init; }
    public DateTime? StartsAt { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public string? PlanCode { get; init; }
    public string? PlanName { get; init; }
    public int RemainingDays { get; init; }
}

public sealed class FreemiumQuotaDto
{
    public int FullTestsPerMonth { get; init; }
    public int PracticeSessionsPerDay { get; init; }
    public int SavedVocabularyMax { get; init; }
}

public sealed class MyOrderDto
{
    public string OrderCode { get; init; } = null!;
    public string PlanName { get; init; } = null!;
    public int DurationDays { get; init; }
    public long AmountVnd { get; init; }
    public string Status { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
    public DateTime? PaidAt { get; init; }
}
