using Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

/// <summary>
/// Mock implementation của IEmailService — chỉ ghi log ra console.
/// Thay bằng SMTP / SendGrid provider ở môi trường production.
/// </summary>
public sealed class MockEmailService(ILogger<MockEmailService> logger) : IEmailService
{
    public Task SendOtpEmailAsync(string toEmail, string otpCode, CancellationToken ct = default)
    {
        logger.LogInformation(
            "[MockEmail] Gửi OTP đến {Email}: mã = {OtpCode}",
            toEmail,
            otpCode);

        Console.WriteLine($"[MockEmail] Sending OTP {otpCode} to {toEmail}");

        return Task.CompletedTask;
    }
}
