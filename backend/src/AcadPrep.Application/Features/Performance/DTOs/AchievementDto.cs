using System;

namespace AcadPrep.Application.Features.Performance.DTOs;

public class AchievementDto
{
    public int AchievementId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public bool IsUnlocked { get; set; }
    public DateTime? UnlockedAt { get; set; }
}
