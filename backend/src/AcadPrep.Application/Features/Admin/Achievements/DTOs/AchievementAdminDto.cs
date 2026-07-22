namespace AcadPrep.Application.Features.Admin.Achievements.DTOs;

public class AchievementAdminDto
{
    public int AchievementId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public string ConditionType { get; set; } = string.Empty;
    public int ConditionValue { get; set; }
}
