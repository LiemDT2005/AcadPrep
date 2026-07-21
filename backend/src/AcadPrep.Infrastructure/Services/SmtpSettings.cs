namespace Infrastructure.Services;

/// <summary>
/// Cấu hình SMTP — bind từ section "Smtp" trong appsettings.
/// Giá trị thật được inject qua User Secrets hoặc environment variables
/// (không hardcode credential vào bất kỳ file nào được commit).
/// </summary>
public sealed class SmtpSettings
{
    /// <summary>SMTP server hostname, ví dụ: smtp.gmail.com</summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>SMTP port, thường là 587 (StartTLS) hoặc 465 (SSL).</summary>
    public int Port { get; set; } = 587;

    /// <summary>Tên đăng nhập SMTP (email hoặc username).</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>Mật khẩu SMTP (App Password với Gmail).</summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>Địa chỉ email hiển thị trong trường "From".</summary>
    public string FromEmail { get; set; } = string.Empty;

    /// <summary>Tên hiển thị trong trường "From", ví dụ: "AcadPrep Team".</summary>
    public string FromName { get; set; } = "AcadPrep";

    /// <summary>Bật/tắt SSL/TLS. Mặc định true.</summary>
    public bool EnableSsl { get; set; } = true;
}
