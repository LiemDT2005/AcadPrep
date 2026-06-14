using System;

namespace Domain.Entities;

public class UserAchievement
{
    public int UserId { get; set; }
    public int AchievementId { get; set; }
    public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Achievement Achievement { get; set; } = null!;
}
