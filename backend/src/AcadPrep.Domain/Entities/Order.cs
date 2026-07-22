using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class Order : BaseEntity<int>, IAuditable
{
    public int UserId { get; set; }
    public int PlanId { get; set; }
    /// <summary>Mã tham chiếu gửi VNPay (vnp_TxnRef) — unique.</summary>
    public string OrderCode { get; set; } = null!;
    public long AmountVnd { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public PaymentProvider PaymentProvider { get; set; } = PaymentProvider.VNPay;
    public string? ProviderTxnId { get; set; }
    public string? ProviderResponseCode { get; set; }
    public string? ProviderBankCode { get; set; }
    public string? ProviderRawPayload { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual Plan Plan { get; set; } = null!;
    public virtual UserSubscription? Subscription { get; set; }
}
