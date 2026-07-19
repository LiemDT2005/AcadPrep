using AcadPrep.Application.Common.Models;
using Application.Common.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Application.Features.Auth.Commands.ForgotPassword;

/// <summary>
/// Handler cho UC-8 Bước 1: Quên mật khẩu — phát OTP reset.
/// Anti-enumeration: luôn trả Result.Success() bất kể email có tồn tại hay không,
/// tránh để lộ thông tin về tài khoản qua response khác nhau.
/// OTP được lưu với prefix riêng "pwd-reset-otp:{email}" để tách biệt với
/// OTP đăng ký/reactivation ("otp:{email}") — tránh xung đột khi user thao tác đồng thời.
/// </summary>
internal sealed class ForgotPasswordCommandHandler(
    IAppDbContext db,
    ICacheService cache,
    IEmailService emailService,
    TimeProvider timeProvider)
    : IRequestHandler<ForgotPasswordCommand, Result<ForgotPasswordResultDto>>
{
    private const string PwdResetOtpPrefix = "pwd-reset-otp:";
    private static readonly TimeSpan OtpTtl = TimeSpan.FromMinutes(5);

    private const string AntiEnumerationMessage =
        "Nếu email của bạn tồn tại trong hệ thống, một mã OTP đã được gửi. Vui lòng kiểm tra hộp thư đến (và thư mục Spam).";

    public async Task<Result<ForgotPasswordResultDto>> Handle(
        ForgotPasswordCommand request,
        CancellationToken cancellationToken)
    {
        // ── Bước 1: Tìm user theo email (AsNoTracking — chỉ đọc) ──────────────
        // KHÔNG trả Failure ngay nếu không tìm thấy — anti-enumeration.
        var user = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        // ── Bước 2: Chỉ gửi OTP nếu user tồn tại VÀ đang Active ─────────────
        // Nếu email không tồn tại hoặc tài khoản không Active (Inactive/Suspended):
        // tiếp tục trả cùng 1 thông báo thành công — không tiết lộ trạng thái.
        if (user is not null && user.Status == UserStatus.Active)
        {
            // ── Bước 3: Sinh OTP 6 số bằng CSPRNG ─────────────────────────
            var otpCode = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

            // ── Bước 4: Lưu OTP vào cache với prefix riêng, TTL 5 phút ────
            var cacheKey = $"{PwdResetOtpPrefix}{request.Email}";
            var expiresAt = timeProvider.GetUtcNow().UtcDateTime.AddMinutes(5);

            var entry = new PwdResetOtpEntry(
                Email:        request.Email,
                OtpCode:      otpCode,
                ExpiresAtUtc: expiresAt);

            await cache.SetAsync(cacheKey, entry, OtpTtl, cancellationToken);

            // ── Bước 5: Gửi OTP qua email ──────────────────────────────────
            await emailService.SendOtpEmailAsync(request.Email, otpCode, cancellationToken);
        }

        // ── Bước 6: Luôn trả thành công (anti-enumeration) ───────────────────
        return Result<ForgotPasswordResultDto>.Success(
            new ForgotPasswordResultDto(AntiEnumerationMessage));
    }
}

