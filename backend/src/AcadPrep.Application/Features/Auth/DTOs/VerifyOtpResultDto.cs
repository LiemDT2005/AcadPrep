namespace Application.Features.Auth.DTOs;

public enum OtpVerificationStatus
{
    Success,
    OtpMismatch,
    OtpExpired
}

public record VerifyOtpResultDto
{
    public OtpVerificationStatus Status { get; init; }
    public int RemainingAttempts { get; init; }
}
