using System;

namespace Domain.Entities;

public class AuditLog
{
    public int LogId { get; set; }
    public int? UserId { get; set; }
    public string Action { get; set; } = null!;
    public string? TableAffected { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual User? User { get; set; }
}
