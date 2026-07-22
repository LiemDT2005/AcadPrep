using System.Text.Json;
using AcadPrep.Application.Common.Models;
using Application.Common.Interfaces;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Billing.Commands.ProcessVNPayIpn;

public sealed class ProcessVNPayIpnCommandHandler
    : IRequestHandler<ProcessVNPayIpnCommand, Result<ProcessVNPayIpnResultDto>>
{
    private readonly IAppDbContext _context;
    private readonly IVNPayService _vnPay;
    private readonly ISubscriptionActivationService _activation;
    private readonly INotificationService _notifications;
    private readonly TimeProvider _timeProvider;

    public ProcessVNPayIpnCommandHandler(
        IAppDbContext context,
        IVNPayService vnPay,
        ISubscriptionActivationService activation,
        INotificationService notifications,
        TimeProvider timeProvider)
    {
        _context = context;
        _vnPay = vnPay;
        _activation = activation;
        _notifications = notifications;
        _timeProvider = timeProvider;
    }

    public async Task<Result<ProcessVNPayIpnResultDto>> Handle(
        ProcessVNPayIpnCommand request,
        CancellationToken cancellationToken)
    {
        var parsed = _vnPay.ParseAndValidate(request.Query);
        var payloadJson = JsonSerializer.Serialize(parsed.Raw);
        var now = _timeProvider.GetUtcNow().UtcDateTime;

        var webhook = new PaymentWebhookLog
        {
            Provider = nameof(PaymentProvider.VNPay),
            OrderCode = parsed.OrderCode,
            Payload = payloadJson,
            SignatureValid = parsed.IsSignatureValid,
            Processed = false,
            ReceivedAt = now
        };
        _context.PaymentWebhookLogs.Add(webhook);
        await _context.SaveChangesAsync(cancellationToken);

        if (!parsed.IsSignatureValid)
        {
            webhook.ProcessResult = "Invalid signature";
            await _context.SaveChangesAsync(cancellationToken);

            return Result<ProcessVNPayIpnResultDto>.Success(new ProcessVNPayIpnResultDto
            {
                RspCode = "97",
                Message = "Invalid signature",
                OrderCode = parsed.OrderCode
            });
        }

        if (string.IsNullOrWhiteSpace(parsed.OrderCode))
        {
            webhook.ProcessResult = "Missing order code";
            await _context.SaveChangesAsync(cancellationToken);
            return Result<ProcessVNPayIpnResultDto>.Success(new ProcessVNPayIpnResultDto
            {
                RspCode = "01",
                Message = "Order not found"
            });
        }

        var order = await _context.Orders
            .Include(o => o.Plan)
            .FirstOrDefaultAsync(o => o.OrderCode == parsed.OrderCode, cancellationToken);

        if (order is null)
        {
            webhook.ProcessResult = "Order not found";
            await _context.SaveChangesAsync(cancellationToken);
            return Result<ProcessVNPayIpnResultDto>.Success(new ProcessVNPayIpnResultDto
            {
                RspCode = "01",
                Message = "Order not found",
                OrderCode = parsed.OrderCode
            });
        }

        if (order.Status == OrderStatus.Paid)
        {
            var existingSub = await _context.UserSubscriptions
                .AsNoTracking()
                .Where(s => s.SourceOrderId == order.Id)
                .OrderByDescending(s => s.ExpiresAt)
                .FirstOrDefaultAsync(cancellationToken);

            webhook.Processed = true;
            webhook.ProcessResult = "Already paid (idempotent)";
            await _context.SaveChangesAsync(cancellationToken);

            return Result<ProcessVNPayIpnResultDto>.Success(new ProcessVNPayIpnResultDto
            {
                RspCode = "00",
                Message = "Confirm Success",
                PaymentSucceeded = true,
                OrderCode = order.OrderCode,
                SubscriptionExpiresAt = existingSub?.ExpiresAt
            });
        }

        if (parsed.AmountVnd is long amount && amount != order.AmountVnd)
        {
            webhook.ProcessResult = $"Amount mismatch: expected {order.AmountVnd}, got {amount}";
            await _context.SaveChangesAsync(cancellationToken);

            return Result<ProcessVNPayIpnResultDto>.Success(new ProcessVNPayIpnResultDto
            {
                RspCode = "04",
                Message = "Invalid amount",
                OrderCode = order.OrderCode
            });
        }

        order.ProviderResponseCode = parsed.ResponseCode;
        order.ProviderTxnId = parsed.TransactionNo;
        order.ProviderBankCode = parsed.BankCode;
        order.ProviderRawPayload = payloadJson;
        order.LastModifiedAt = now;

        if (!parsed.IsSuccess)
        {
            order.Status = string.Equals(parsed.ResponseCode, "24", StringComparison.Ordinal)
                ? OrderStatus.Cancelled
                : OrderStatus.Failed;

            webhook.Processed = true;
            webhook.ProcessResult = parsed.Message;
            await _context.SaveChangesAsync(cancellationToken);

            return Result<ProcessVNPayIpnResultDto>.Success(new ProcessVNPayIpnResultDto
            {
                RspCode = "00",
                Message = "Confirm Success",
                PaymentSucceeded = false,
                OrderCode = order.OrderCode
            });
        }

        if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Expired)
        {
            webhook.ProcessResult = $"Unexpected status {order.Status}";
            await _context.SaveChangesAsync(cancellationToken);
            return Result<ProcessVNPayIpnResultDto>.Success(new ProcessVNPayIpnResultDto
            {
                RspCode = "02",
                Message = "Order already confirmed",
                OrderCode = order.OrderCode
            });
        }

        order.Status = OrderStatus.Paid;
        order.PaidAt = now;
        await _context.SaveChangesAsync(cancellationToken);

        var subscription = await _activation.ActivateFromPaidOrderAsync(order, cancellationToken);

        try
        {
            await _notifications.CreateAsync(
                order.UserId,
                "Pro payment successful",
                $"{order.Plan.Name} is active until {subscription.ExpiresAt:dd/MM/yyyy HH:mm} (UTC).",
                NotificationType.PaymentSucceeded,
                "/Pricing",
                cancellationToken);
        }
        catch
        {
            // Không fail payment nếu notification lỗi.
        }

        webhook.Processed = true;
        webhook.ProcessResult = "Paid + subscription activated";
        await _context.SaveChangesAsync(cancellationToken);

        return Result<ProcessVNPayIpnResultDto>.Success(new ProcessVNPayIpnResultDto
        {
            RspCode = "00",
            Message = "Confirm Success",
            PaymentSucceeded = true,
            OrderCode = order.OrderCode,
            SubscriptionExpiresAt = subscription.ExpiresAt
        });
    }
}
