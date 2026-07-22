using AcadPrep.Application.Common.Models;
using Application.Common.Interfaces;
using Application.Common.Options;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AcadPrep.Application.Features.Billing.Commands.CreatePaymentOrder;

public sealed class CreatePaymentOrderCommandValidator : AbstractValidator<CreatePaymentOrderCommand>
{
    public CreatePaymentOrderCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.PlanId).GreaterThan(0);
        RuleFor(x => x.AbsoluteReturnUrl).NotEmpty();
    }
}

public sealed class CreatePaymentOrderCommandHandler
    : IRequestHandler<CreatePaymentOrderCommand, Result<CreatePaymentOrderResultDto>>
{
    private readonly IAppDbContext _context;
    private readonly IVNPayService _vnPay;
    private readonly VNPaySettings _settings;
    private readonly TimeProvider _timeProvider;

    public CreatePaymentOrderCommandHandler(
        IAppDbContext context,
        IVNPayService vnPay,
        IOptions<VNPaySettings> settings,
        TimeProvider timeProvider)
    {
        _context = context;
        _vnPay = vnPay;
        _settings = settings.Value;
        _timeProvider = timeProvider;
    }

    public async Task<Result<CreatePaymentOrderResultDto>> Handle(
        CreatePaymentOrderCommand request,
        CancellationToken cancellationToken)
    {
        if (!_settings.IsConfigured)
        {
            return Result<CreatePaymentOrderResultDto>.Failure(
                "VNPay is not configured. Please contact an administrator.");
        }

        var plan = await _context.Plans
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.PlanId && p.IsActive, cancellationToken);

        if (plan is null)
        {
            return Result<CreatePaymentOrderResultDto>.Failure("This plan does not exist or is no longer available.");
        }

        var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);
        if (!userExists)
        {
            return Result<CreatePaymentOrderResultDto>.Failure("Invalid account.");
        }

        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var orderCode = BuildOrderCode(request.UserId, now);

        var order = new Order
        {
            UserId = request.UserId,
            PlanId = plan.Id,
            OrderCode = orderCode,
            AmountVnd = plan.PriceVnd,
            Status = OrderStatus.Pending,
            PaymentProvider = PaymentProvider.VNPay,
            ExpiresAt = now.AddMinutes(_settings.OrderExpiryMinutes),
            CreatedAt = now
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        string paymentUrl;
        try
        {
            paymentUrl = _vnPay.CreatePaymentUrl(new VNPayPaymentRequest
            {
                OrderCode = order.OrderCode,
                AmountVnd = order.AmountVnd,
                OrderInfo = $"AcadPrep Pro {plan.Code} {order.OrderCode}",
                ClientIp = request.ClientIp,
                CreatedAtUtc = now,
                ReturnUrlOverride = request.AbsoluteReturnUrl
            });
        }
        catch (Exception ex)
        {
            order.Status = OrderStatus.Failed;
            order.LastModifiedAt = now;
            await _context.SaveChangesAsync(cancellationToken);
            return Result<CreatePaymentOrderResultDto>.Failure($"Could not create the payment URL: {ex.Message}");
        }

        return Result<CreatePaymentOrderResultDto>.Success(new CreatePaymentOrderResultDto
        {
            OrderId = order.Id,
            OrderCode = order.OrderCode,
            PaymentUrl = paymentUrl
        });
    }

    private static string BuildOrderCode(int userId, DateTime utcNow)
    {
        // Unique trong ngày theo yêu cầu VNPay; thêm random để chống race.
        var stamp = utcNow.ToString("yyMMddHHmmss");
        var rand = Random.Shared.Next(1000, 9999);
        return $"AP{stamp}{userId % 10000:D4}{rand}";
    }
}
