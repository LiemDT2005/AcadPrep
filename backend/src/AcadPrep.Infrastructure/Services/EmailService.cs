using Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class EmailService(ILogger<EmailService> logger) : IEmailService
{
    public Task SendOtpAsync(string toEmail, string otpCode, CancellationToken ct = default)
    {
        logger.LogInformation(
            "[EmailService] Sending OTP to {Email}: {OtpCode}",
            toEmail,
            otpCode);

        return Task.CompletedTask;
    }
}
