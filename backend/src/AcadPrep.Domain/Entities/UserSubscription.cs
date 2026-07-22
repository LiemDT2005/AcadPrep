using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class UserSubscription : BaseEntity<int>, IAuditable
{
    public int UserId { get; set; }
    public int PlanId { get; set; }
    public int? SourceOrderId { get; set; }
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
    public DateTime StartsAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual Plan Plan { get; set; } = null!;
    public virtual Order? SourceOrder { get; set; }

    public bool IsActiveAt(DateTime utcNow) =>
        Status == SubscriptionStatus.Active && ExpiresAt > utcNow;
}
