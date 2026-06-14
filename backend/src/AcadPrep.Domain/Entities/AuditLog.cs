using Domain.Common;

namespace Domain.Entities;

public class AuditLog : BaseEntity<int>
{
    public int? UserId { get; set; }
    public string Action { get; set; } = null!;
    public string? TableAffected { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual User? User { get; set; }
}
