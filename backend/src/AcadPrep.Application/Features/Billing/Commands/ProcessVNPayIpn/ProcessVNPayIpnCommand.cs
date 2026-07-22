using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.Billing.Commands.ProcessVNPayIpn;

/// <summary>
/// Xử lý IPN (server-to-server) hoặc Return URL — idempotent.
/// </summary>
public record ProcessVNPayIpnCommand(
    IReadOnlyList<KeyValuePair<string, string>> Query,
    bool IsIpn
) : IRequest<Result<ProcessVNPayIpnResultDto>>;

public sealed class ProcessVNPayIpnResultDto
{
    /// <summary>Mã trả về VNPay IPN: 00 = OK, 97 = chữ ký sai, ...</summary>
    public string RspCode { get; init; } = "99";
    public string Message { get; init; } = "Unknown";
    public bool PaymentSucceeded { get; init; }
    public string? OrderCode { get; init; }
    public DateTime? SubscriptionExpiresAt { get; init; }
}
