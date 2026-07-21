namespace AcadPrep.Application.Common.Models;

/// <summary>
/// Cache entry cho OTP reset mật khẩu (UC-8 Quên mật khẩu).
/// Lưu dưới key "pwd-reset-otp:{email}", TTL 5 phút.
/// Tách biệt với OtpCacheEntry (dùng cho đăng ký/reactivation)
/// để tránh xung đột khi user thao tác đồng thời cả 2 luồng.
/// </summary>
public sealed record PwdResetOtpEntry(
    string Email,
    string OtpCode,
    DateTime ExpiresAtUtc);
