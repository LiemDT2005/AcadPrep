namespace AcadPrep.Application.Features.Admin.Accounts.DTOs;

public class AccountDetailDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? AvatarUrl { get; set; }
    public string RoleName { get; set; } = null!;
    public int RoleId { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public bool IsMasterAdmin { get; set; }
    public string? GoogleId { get; set; }

    // Stats
    public int ExamsTaken { get; set; }
    public double AverageScore { get; set; }
    public int CurrentStreak { get; set; }
}
