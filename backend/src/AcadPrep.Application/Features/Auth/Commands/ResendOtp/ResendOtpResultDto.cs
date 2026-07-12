namespace Application.Features.Auth.Commands.ResendOtp;

/// <summary>
/// DTO trả về sau khi gửi lại OTP thành công.
/// </summary>
public sealed record ResendOtpResultDto(string Email);
