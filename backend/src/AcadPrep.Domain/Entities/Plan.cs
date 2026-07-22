using Domain.Common;

namespace Domain.Entities;

public class Plan : BaseEntity<int>, IAuditable
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    /// <summary>Giá VND (số nguyên, không thập phân).</summary>
    public long PriceVnd { get; set; }
    public int DurationDays { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual ICollection<UserSubscription> Subscriptions { get; set; } = new List<UserSubscription>();
}
