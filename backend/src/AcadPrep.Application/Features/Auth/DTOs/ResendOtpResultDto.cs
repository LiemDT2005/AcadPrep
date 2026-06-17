namespace Application.Features.Auth.DTOs;

public record ResendOtpResultDto
{
    public bool IsSuccess { get; init; }
    public bool IsLocked { get; init; }
    public bool IsExpired { get; init; }
}
