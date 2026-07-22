using System.Collections.Generic;

namespace Domain.Entities;

public class Achievement
{
    public int AchievementId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public string ConditionType { get; set; } = string.Empty;
    public int ConditionValue { get; set; }
    
    // Navigation properties
    public virtual ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
}
