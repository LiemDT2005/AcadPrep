namespace Application.Features.Auth.Commands.ForgotPassword;

/// <summary>
/// DTO trả về sau khi gửi OTP reset mật khẩu (UC-8 Bước 1).
/// Luôn chứa message thành công chung, bất kể email có tồn tại hay không
/// (anti-enumeration — không tiết lộ trạng thái tài khoản qua response).
/// </summary>
public sealed record ForgotPasswordResultDto(string Message);
