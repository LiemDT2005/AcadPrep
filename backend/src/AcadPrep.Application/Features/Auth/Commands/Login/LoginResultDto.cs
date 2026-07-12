namespace Application.Features.Auth.Commands.Login;

/// <summary>
/// DTO trả về sau khi xác thực thành công — dùng chung cho cả Login bằng Email (UC-5.1)
/// và Login bằng Google (UC-5.2).
/// PageModel dùng dữ liệu này để dựng ClaimsPrincipal và gọi HttpContext.SignInAsync.
/// </summary>
public sealed record LoginResultDto(
    int UserId,
    string Email,
    string FullName,
    string Role,
    /// <summary>
    /// true khi tài khoản cần xác minh (Status == Inactive trong flow email).
    /// Khi true, PageModel KHÔNG phát cookie — hiển thị thông báo "Cần xác minh tài khoản".
    /// </summary>
    bool RequiresVerification = false
);
