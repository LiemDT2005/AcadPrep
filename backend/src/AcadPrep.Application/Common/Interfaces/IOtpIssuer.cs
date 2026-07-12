namespace Application.Common.Interfaces;

/// <summary>
/// Abstraction phát OTP — sinh mã, lưu Redis, gửi email.
/// Trả về false nếu email đang bị khóa (key "otp-lock:{email}" còn hiệu lực).
/// </summary>
public interface IOtpIssuer
{
    /// <summary>
    /// Phát OTP cho email được chỉ định.
    /// </summary>
    /// <param name="email">Địa chỉ email nhận OTP.</param>
    /// <param name="isReactivation">true nếu là flow tái kích hoạt tài khoản (Batch B).</param>
    /// <param name="passwordHash">Hash mật khẩu cần lưu tạm (null với flow reactivation).</param>
    /// <param name="fullName">Họ tên người dùng cần lưu tạm (null với flow reactivation).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// true  = OTP đã được sinh, lưu Redis và gửi email thành công.<br/>
    /// false = Email đang bị khóa (key "otp-lock:{email}" còn hiệu lực) —
    ///         không sinh/lưu/gửi OTP gì cả.
    /// </returns>
    Task<bool> IssueOtpAsync(
        string email,
        bool isReactivation,
        string? passwordHash,
        string? fullName,
        CancellationToken ct = default);
}
