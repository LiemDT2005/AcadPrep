using AcadPrep.Application.Common.Models;
using Application.Common.Interfaces;
using Application.Common.Options;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AcadPrep.Application.Features.Billing.Queries.GetPricingPlans;

public sealed class GetPricingPlansQueryHandler
    : IRequestHandler<GetPricingPlansQuery, Result<PricingPageDto>>
{
    private readonly IAppDbContext _context;
    private readonly IBillingAccessService _billing;
    private readonly FreemiumSettings _freemium;

    public GetPricingPlansQueryHandler(
        IAppDbContext context,
        IBillingAccessService billing,
        IOptions<FreemiumSettings> freemium)
    {
        _context = context;
        _billing = billing;
        _freemium = freemium.Value;
    }

    public async Task<Result<PricingPageDto>> Handle(GetPricingPlansQuery request, CancellationToken cancellationToken)
    {
        var plans = await _context.Plans
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.SortOrder)
            .ThenBy(p => p.PriceVnd)
            .Select(p => new PlanDto
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                Description = p.Description,
                PriceVnd = p.PriceVnd,
                DurationDays = p.DurationDays,
                IsHighlighted = p.Code == "pro_quarterly"
            })
            .ToListAsync(cancellationToken);

        SubscriptionStatusDto? subscription = null;
        IReadOnlyList<MyOrderDto> recentOrders = Array.Empty<MyOrderDto>();

        if (request.UserId is int userId)
        {
            var snap = await _billing.GetSubscriptionAsync(userId, cancellationToken);
            subscription = new SubscriptionStatusDto
            {
                IsPro = snap.IsPro,
                StartsAt = snap.StartsAt,
                ExpiresAt = snap.ExpiresAt,
                PlanCode = snap.PlanCode,
                PlanName = snap.PlanName,
                RemainingDays = snap.RemainingDays
            };

            recentOrders = await _context.Orders
                .AsNoTracking()
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .Take(10)
                .Select(o => new MyOrderDto
                {
                    OrderCode = o.OrderCode,
                    PlanName = o.Plan.Name,
                    DurationDays = o.Plan.DurationDays,
                    AmountVnd = o.AmountVnd,
                    Status = o.Status.ToString(),
                    CreatedAt = o.CreatedAt,
                    PaidAt = o.PaidAt
                })
                .ToListAsync(cancellationToken);
        }

        return Result<PricingPageDto>.Success(new PricingPageDto
        {
            Plans = plans,
            Subscription = subscription,
            RecentOrders = recentOrders,
            FreeQuota = new FreemiumQuotaDto
            {
                FullTestsPerMonth = _freemium.FullTestsPerMonth,
                PracticeSessionsPerDay = _freemium.PracticeSessionsPerDay,
                SavedVocabularyMax = _freemium.SavedVocabularyMax
            }
        });
    }
}
