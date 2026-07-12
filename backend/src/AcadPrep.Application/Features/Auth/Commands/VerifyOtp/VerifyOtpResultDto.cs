namespace Application.Features.Auth.Commands.VerifyOtp;

/// <summary>
/// DTO trả về sau khi xác minh OTP thành công.
/// PageModel dùng Email để dựng redirect URL hoặc hiển thị thông báo.
/// </summary>
public sealed record VerifyOtpResultDto(string Email);
