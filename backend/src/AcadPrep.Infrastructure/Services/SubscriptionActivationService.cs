using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

/// <summary>
/// Kích hoạt / gia hạn Pro sau khi thanh toán thành công hoặc Admin grant.
/// Idempotent theo Order (không tạo subscription trùng cho cùng SourceOrderId).
/// </summary>
public sealed class SubscriptionActivationService : ISubscriptionActivationService
{
    private readonly IAppDbContext _context;
    private readonly TimeProvider _timeProvider;

    public SubscriptionActivationService(IAppDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<UserSubscription> ActivateFromPaidOrderAsync(Order order, CancellationToken cancellationToken = default)
    {
        var existing = await _context.UserSubscriptions
            .FirstOrDefaultAsync(s => s.SourceOrderId == order.Id, cancellationToken);

        if (existing is not null)
        {
            return existing;
        }

        var plan = order.Plan ?? await _context.Plans
            .FirstAsync(p => p.Id == order.PlanId, cancellationToken);

        return await ExtendOrCreateAsync(
            order.UserId,
            plan,
            sourceOrderId: order.Id,
            note: null,
            cancellationToken);
    }

    public async Task<UserSubscription> GrantManualAsync(
        int userId,
        int planId,
        string? note,
        CancellationToken cancellationToken = default)
    {
        var plan = await _context.Plans.FirstAsync(p => p.Id == planId, cancellationToken);
        return await ExtendOrCreateAsync(userId, plan, sourceOrderId: null, note, cancellationToken);
    }

    private async Task<UserSubscription> ExtendOrCreateAsync(
        int userId,
        Plan plan,
        int? sourceOrderId,
        string? note,
        CancellationToken cancellationToken)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;

        var active = await _context.UserSubscriptions
            .Where(s => s.UserId == userId && s.Status == SubscriptionStatus.Active && s.ExpiresAt > now)
            .OrderByDescending(s => s.ExpiresAt)
            .FirstOrDefaultAsync(cancellationToken);

        DateTime startsAt;
        DateTime expiresAt;

        if (active is not null)
        {
            // Gia hạn: cộng thêm ngày từ ExpiresAt hiện tại
            startsAt = active.StartsAt;
            expiresAt = active.ExpiresAt.AddDays(plan.DurationDays);
            active.Status = SubscriptionStatus.Expired;
            active.LastModifiedAt = now;
        }
        else
        {
            startsAt = now;
            expiresAt = now.AddDays(plan.DurationDays);
        }

        var subscription = new UserSubscription
        {
            UserId = userId,
            PlanId = plan.Id,
            SourceOrderId = sourceOrderId,
            Status = SubscriptionStatus.Active,
            StartsAt = startsAt,
            ExpiresAt = expiresAt,
            Note = note,
            CreatedAt = now
        };

        _context.UserSubscriptions.Add(subscription);
        await _context.SaveChangesAsync(cancellationToken);
        return subscription;
    }
}
