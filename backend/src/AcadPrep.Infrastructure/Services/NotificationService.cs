using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

/// <summary>
/// Tạo thông báo và lưu ngay vào DB. Title/Message được cắt theo ràng buộc cột
/// (200/1000 ký tự) để tránh lỗi khi bên gọi truyền chuỗi quá dài.
/// </summary>
public class NotificationService : INotificationService
{
    private const int MaxTitleLength = 200;
    private const int MaxMessageLength = 1000;
    private const int MaxTypeLength = 50;
    private const int MaxLinkLength = 500;

    private readonly IAppDbContext _context;
    private readonly TimeProvider _timeProvider;

    public NotificationService(IAppDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task CreateAsync(
        int userId,
        string title,
        string message,
        string type,
        string? linkUrl = null,
        CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = Truncate(title, MaxTitleLength),
            Message = Truncate(message, MaxMessageLength),
            Type = Truncate(type, MaxTypeLength),
            LinkUrl = string.IsNullOrWhiteSpace(linkUrl) ? null : Truncate(linkUrl, MaxLinkLength),
            IsRead = false,
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task CreateForRoleAsync(
        string roleName,
        string title,
        string message,
        string type,
        string? linkUrl = null,
        CancellationToken cancellationToken = default)
    {
        var userIds = await _context.Users
            .Where(u => u.Role.RoleName == roleName)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        if (userIds.Count == 0)
            return;

        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var safeTitle = Truncate(title, MaxTitleLength);
        var safeMessage = Truncate(message, MaxMessageLength);
        var safeType = Truncate(type, MaxTypeLength);
        var safeLink = string.IsNullOrWhiteSpace(linkUrl) ? null : Truncate(linkUrl, MaxLinkLength);

        foreach (var uid in userIds)
        {
            _context.Notifications.Add(new Notification
            {
                UserId = uid,
                Title = safeTitle,
                Message = safeMessage,
                Type = safeType,
                LinkUrl = safeLink,
                IsRead = false,
                CreatedAt = now
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
