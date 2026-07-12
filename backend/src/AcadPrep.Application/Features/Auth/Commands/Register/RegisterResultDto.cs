namespace Application.Features.Auth.Commands.Register;

/// <summary>
/// DTO trả về sau khi đăng ký thành công ở bước 1 (UC-1).
/// Chỉ chứa Email để PageModel biết OTP đã được gửi tới địa chỉ nào.
/// Tài khoản chưa được tạo trong SQL — chỉ được tạo sau khi xác minh OTP (UC-2).
/// </summary>
public sealed record RegisterResultDto(string Email);
