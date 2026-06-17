namespace Application.Common.Interfaces;

public interface IEmailService
{
    Task SendOtpAsync(string toEmail, string otpCode, CancellationToken ct = default);
}
