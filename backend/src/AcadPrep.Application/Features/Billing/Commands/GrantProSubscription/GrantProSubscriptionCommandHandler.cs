using AcadPrep.Application.Common.Models;
using Application.Common.Interfaces;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Billing.Commands.GrantProSubscription;

public sealed class GrantProSubscriptionCommandValidator : AbstractValidator<GrantProSubscriptionCommand>
{
    public GrantProSubscriptionCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.PlanId).GreaterThan(0);
        RuleFor(x => x.AdminUserId).GreaterThan(0);
    }
}

public sealed class GrantProSubscriptionCommandHandler
    : IRequestHandler<GrantProSubscriptionCommand, Result<GrantProSubscriptionResultDto>>
{
    private readonly IAppDbContext _context;
    private readonly ISubscriptionActivationService _activation;
    private readonly INotificationService _notifications;
    private readonly TimeProvider _timeProvider;

    public GrantProSubscriptionCommandHandler(
        IAppDbContext context,
        ISubscriptionActivationService activation,
        INotificationService notifications,
        TimeProvider timeProvider)
    {
        _context = context;
        _activation = activation;
        _notifications = notifications;
        _timeProvider = timeProvider;
    }

    public async Task<Result<GrantProSubscriptionResultDto>> Handle(
        GrantProSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user is null)
        {
            return Result<GrantProSubscriptionResultDto>.Failure("User not found.");
        }

        var plan = await _context.Plans.FirstOrDefaultAsync(p => p.Id == request.PlanId && p.IsActive, cancellationToken);
        if (plan is null)
        {
            return Result<GrantProSubscriptionResultDto>.Failure("Invalid plan.");
        }

        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var orderCode = $"MANUAL{now:yyMMddHHmmss}{request.UserId % 10000:D4}{Random.Shared.Next(100, 999)}";

        var order = new Order
        {
            UserId = request.UserId,
            PlanId = plan.Id,
            OrderCode = orderCode,
            AmountVnd = 0,
            Status = OrderStatus.Paid,
            PaymentProvider = PaymentProvider.Manual,
            PaidAt = now,
            ExpiresAt = now,
            CreatedAt = now,
            ProviderRawPayload = $"Granted by admin {request.AdminUserId}. Note: {request.Note}"
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        var subscription = await _activation.ActivateFromPaidOrderAsync(order, cancellationToken);
        if (!string.IsNullOrWhiteSpace(request.Note))
        {
            subscription.Note = request.Note;
            subscription.LastModifiedAt = now;
            await _context.SaveChangesAsync(cancellationToken);
        }

        await _notifications.CreateAsync(
            request.UserId,
            "You have been upgraded to Pro",
            $"{plan.Name} is active until {subscription.ExpiresAt:dd/MM/yyyy HH:mm} (UTC).",
            NotificationType.SubscriptionGranted,
            "/Pricing",
            cancellationToken);

        return Result<GrantProSubscriptionResultDto>.Success(new GrantProSubscriptionResultDto
        {
            SubscriptionId = subscription.Id,
            ExpiresAt = subscription.ExpiresAt
        });
    }
}
