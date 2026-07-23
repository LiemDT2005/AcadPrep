namespace Application.Common.Interfaces;

using Domain.Entities;

/// <summary>
/// Kích hoạt / gia hạn Pro sau thanh toán hoặc Admin grant.
/// </summary>
public interface ISubscriptionActivationService
{
    Task<UserSubscription> ActivateFromPaidOrderAsync(Order order, CancellationToken cancellationToken = default);

    Task<UserSubscription> GrantManualAsync(
        int userId,
        int planId,
        string? note,
        CancellationToken cancellationToken = default);
}
