using Application.Common.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Infrastructure.Services;

/// <summary>
/// SMTP email service thật — dùng MailKit với StartTLS (port 587).
/// Được DI inject ở môi trường Non-Development thay cho MockEmailService.
/// Credential KHÔNG được hardcode — inject qua IOptions&lt;SmtpSettings&gt;,
/// giá trị thật đến từ User Secrets hoặc environment variables.
/// </summary>
public sealed class SmtpEmailService(
    IOptions<SmtpSettings> smtpOptions,
    ILogger<SmtpEmailService> logger) : IEmailService
{
    private readonly SmtpSettings _smtp = smtpOptions.Value;

    public async Task SendOtpEmailAsync(string toEmail, string otpCode, CancellationToken ct = default)
    {
        logger.LogInformation(
            "[SmtpEmail] Đang gửi OTP tới {Email} qua {Host}:{Port}",
            toEmail, _smtp.Host, _smtp.Port);

        try
        {
            // ── Xây dựng MimeMessage ─────────────────────────────────────────
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_smtp.FromName, _smtp.FromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = "AcadPrep - Mã xác thực OTP của bạn";

            message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = BuildHtmlBody(otpCode)
            };

            // ── Kết nối + Auth + Gửi ────────────────────────────────────────
            using var client = new SmtpClient();

            await client.ConnectAsync(_smtp.Host, _smtp.Port, SecureSocketOptions.StartTls, ct);
            await client.AuthenticateAsync(_smtp.Username, _smtp.Password, ct);
            await client.SendAsync(message, ct);
            await client.DisconnectAsync(quit: true, ct);

            logger.LogInformation(
                "[SmtpEmail] Gửi OTP thành công tới {Email}",
                toEmail);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "[SmtpEmail] Gửi email thất bại tới {Email} qua {Host}:{Port}",
                toEmail, _smtp.Host, _smtp.Port);

            // Rethrow để caller biết email gửi thất bại — không nuốt lỗi.
            throw;
        }
    }

    // ── HTML email body ──────────────────────────────────────────────────────
    private static string BuildHtmlBody(string otpCode) => $"""
        <!DOCTYPE html>
        <html lang="vi">
        <head>
            <meta charset="utf-8" />
            <meta name="viewport" content="width=device-width, initial-scale=1.0" />
        </head>
        <body style="margin:0;padding:0;background:#f4f4f8;font-family:'Segoe UI',Arial,sans-serif;">
            <table width="100%" cellpadding="0" cellspacing="0" style="background:#f4f4f8;padding:40px 0;">
                <tr>
                    <td align="center">
                        <table width="520" cellpadding="0" cellspacing="0"
                               style="background:#ffffff;border-radius:16px;overflow:hidden;
                                      box-shadow:0 4px 24px rgba(30,0,169,0.10);">

                            <!-- Header -->
                            <tr>
                                <td style="background:linear-gradient(135deg,#1e00a9 0%,#3525cd 100%);
                                           padding:32px 40px;text-align:center;">
                                    <h1 style="margin:0;color:#ffffff;font-size:24px;font-weight:700;
                                               letter-spacing:0.5px;">AcadPrep</h1>
                                    <p style="margin:6px 0 0;color:rgba(255,255,255,0.75);font-size:13px;">
                                        Luyện thi TOEIC thông minh
                                    </p>
                                </td>
                            </tr>

                            <!-- Body -->
                            <tr>
                                <td style="padding:36px 40px;">
                                    <p style="margin:0 0 8px;color:#1a1a2e;font-size:16px;font-weight:600;">
                                        Xin chào!
                                    </p>
                                    <p style="margin:0 0 24px;color:#464555;font-size:14px;line-height:1.6;">
                                        Chúng tôi nhận được yêu cầu của bạn. Vui lòng dùng mã OTP dưới đây
                                        để tiếp tục. Mã có hiệu lực trong <strong>5 phút</strong>.
                                    </p>

                                    <!-- OTP Box -->
                                    <div style="background:#f0edff;border:2px dashed #3525cd;
                                                border-radius:12px;padding:20px;text-align:center;
                                                margin-bottom:24px;">
                                        <p style="margin:0 0 6px;color:#464555;font-size:12px;
                                                   letter-spacing:1px;text-transform:uppercase;">
                                            Mã xác thực OTP
                                        </p>
                                        <p style="margin:0;color:#1e00a9;font-size:38px;font-weight:800;
                                                   letter-spacing:8px;font-family:'Courier New',monospace;">
                                            {otpCode}
                                        </p>
                                    </div>

                                    <p style="margin:0;color:#787680;font-size:13px;line-height:1.5;">
                                        ⚠️ <strong>Không chia sẻ mã này</strong> với bất kỳ ai, kể cả nhân viên AcadPrep.<br/>
                                        Nếu bạn không thực hiện yêu cầu này, hãy bỏ qua email này.
                                    </p>
                                </td>
                            </tr>

                            <!-- Footer -->
                            <tr>
                                <td style="background:#f8f7fc;padding:20px 40px;text-align:center;
                                           border-top:1px solid #e8e6f0;">
                                    <p style="margin:0;color:#9490a8;font-size:12px;">
                                        © {DateTime.UtcNow.Year} AcadPrep · Mọi quyền được bảo lưu
                                    </p>
                                </td>
                            </tr>

                        </table>
                    </td>
                </tr>
            </table>
        </body>
        </html>
        """;
}
