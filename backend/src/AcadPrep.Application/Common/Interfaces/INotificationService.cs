namespace Application.Common.Interfaces;

/// <summary>
/// Điểm chung để mọi module tạo thông báo trong hộp thư người dùng (UC-15).
/// Bên gọi chịu trách nhiệm truyền đúng <paramref name="userId"/> người nhận và
/// <paramref name="type"/> lấy từ <c>Domain.Constants.NotificationType</c>.
/// </summary>
public interface INotificationService
{
    Task CreateAsync(
        int userId,
        string title,
        string message,
        string type,
        string? linkUrl = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tạo cùng một thông báo cho tất cả người dùng thuộc <paramref name="roleName"/>
    /// (ví dụ broadcast cảnh báo hệ thống cho toàn bộ Admin). Lưu một lần duy nhất.
    /// </summary>
    Task CreateForRoleAsync(
        string roleName,
        string title,
        string message,
        string type,
        string? linkUrl = null,
        CancellationToken cancellationToken = default);
}
