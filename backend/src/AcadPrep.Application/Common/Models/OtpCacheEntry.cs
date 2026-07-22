namespace AcadPrep.Application.Common.Models;

/// <summary>
/// Dữ liệu lưu trong Redis dưới key "otp:{email}" trong suốt quá trình xác minh OTP.
/// TTL = 5 phút (do OtpIssuer set khi ghi vào cache).
/// FailedAttempts và ExpiresAtUtc được dùng ở Batch B (UC-2 Verify OTP).
/// </summary>
public sealed record OtpCacheEntry(
    string Email,
    string OtpCode,
    bool IsReactivation,
    string? PasswordHash,
    string? FullName,
    DateTime ExpiresAtUtc,
    int FailedAttempts = 0);
