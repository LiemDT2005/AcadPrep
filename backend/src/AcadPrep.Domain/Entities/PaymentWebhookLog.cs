using Domain.Common;

namespace Domain.Entities;

/// <summary>
/// Audit log raw IPN/return từ cổng thanh toán — phục vụ đối soát production.
/// </summary>
public class PaymentWebhookLog : BaseEntity<long>
{
    public string Provider { get; set; } = "VNPay";
    public string? OrderCode { get; set; }
    public string Payload { get; set; } = null!;
    public bool SignatureValid { get; set; }
    public bool Processed { get; set; }
    public string? ProcessResult { get; set; }
    public DateTime ReceivedAt { get; set; }
}
