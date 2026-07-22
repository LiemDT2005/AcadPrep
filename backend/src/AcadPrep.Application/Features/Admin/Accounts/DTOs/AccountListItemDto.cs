namespace AcadPrep.Application.Features.Admin.Accounts.DTOs;

public class AccountListItemDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? AvatarUrl { get; set; }
    public string RoleName { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public bool IsMasterAdmin { get; set; }
}
