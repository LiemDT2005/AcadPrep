namespace Application.Common.Interfaces;

/// <summary>
/// Abstraction gửi email OTP — Application layer không phụ thuộc trực tiếp
/// vào bất kỳ mail provider cụ thể nào.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Gửi email chứa mã OTP tới địa chỉ email được chỉ định.
    /// </summary>
    Task SendOtpEmailAsync(string toEmail, string otpCode, CancellationToken ct = default);
}
