namespace Application.Features.Auth.DTOs;

public record StagingRegistrationData
{
    public string Email { get; init; } = null!;
    public string PasswordHash { get; init; } = null!;
    public string FullName { get; init; } = null!;
    public int RoleId { get; init; }
    public string OtpCode { get; set; } = null!;
    public int FailedAttempts { get; set; }
}
