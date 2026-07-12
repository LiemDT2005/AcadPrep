using System.Security.Cryptography;
using AcadPrep.Application.Common.Models;
using Application.Common.Interfaces;

namespace Infrastructure.Services;

/// <summary>
/// Sinh OTP 6 số bằng RandomNumberGenerator, lưu vào Redis, gửi email.
/// Trả về false ngay nếu email đang trong thời gian khóa (otp-lock:{email}).
/// </summary>
public sealed class OtpIssuer(
    ICacheService cache,
    IEmailService emailService,
    TimeProvider timeProvider) : IOtpIssuer
{
    private const string OtpKeyPrefix  = "otp:";
    private const string LockKeyPrefix = "otp-lock:";
    private static readonly TimeSpan OtpTtl = TimeSpan.FromMinutes(5);

    public async Task<bool> IssueOtpAsync(
        string email,
        bool isReactivation,
        string? passwordHash,
        string? fullName,
        CancellationToken ct = default)
    {
        // ── Bước 1: Kiểm tra khóa ──────────────────────────────────────────
        // Nếu otp-lock:{email} tồn tại → email đang bị khóa do nhập sai quá số lần.
        // Trả về false ngay, không sinh/lưu/gửi OTP gì cả.
        //
        // Đọc kiểu bool để khớp với Batch B (SetAsync<bool>(lockKey, true, ...)).
        // GetAsync<string> sẽ ném JsonException khi deserialize JSON `true`,
        // bị RedisCacheService nuốt âm thầm → lockFlag luôn null → check bị bypass.
        // default(bool) = false → "key không tồn tại = không bị khóa" — đúng ngữ nghĩa.
        var lockKey  = $"{LockKeyPrefix}{email}";
        var isLocked = await cache.GetAsync<bool>(lockKey, ct);
        if (isLocked)
        {
            return false;
        }

        // ── Bước 2: Sinh OTP 6 số bằng RandomNumberGenerator ───────────────
        // Không dùng System.Random — RandomNumberGenerator đảm bảo CSPRNG.
        var otpCode = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

        // ── Bước 3: Tính ExpiresAtUtc qua TimeProvider ─────────────────────
        // Tuyệt đối không dùng DateTime.UtcNow trực tiếp.
        var expiresAt = timeProvider.GetUtcNow().UtcDateTime.AddMinutes(5);

        // ── Bước 4: Xây dựng entry và lưu vào Redis ────────────────────────
        var otpKey = $"{OtpKeyPrefix}{email}";
        var entry  = new OtpCacheEntry(
            Email:          email,
            OtpCode:        otpCode,
            IsReactivation: isReactivation,
            PasswordHash:   passwordHash,
            FullName:       fullName,
            ExpiresAtUtc:   expiresAt,
            FailedAttempts: 0);

        await cache.SetAsync(otpKey, entry, OtpTtl, ct);

        // ── Bước 5: Gửi OTP qua email ──────────────────────────────────────
        await emailService.SendOtpEmailAsync(email, otpCode, ct);

        return true;
    }
}
