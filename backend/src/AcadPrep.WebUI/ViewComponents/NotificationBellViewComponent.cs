using System.Security.Claims;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.WebUI.ViewComponents;

public class NotificationBellViewComponent : ViewComponent
{
    private const int PreviewCount = 5;

    private readonly IAppDbContext _context;

    public NotificationBellViewComponent(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync(string variant = "light")
    {
        var userIdValue = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdValue, out var userId))
        {
            return View(new NotificationBellViewModel { UnreadCount = 0, Variant = variant });
        }

        var recent = await _context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(PreviewCount)
            .Select(n => new NotificationPreviewItem
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                LinkUrl = n.LinkUrl,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync();

        var unread = await _context.Notifications
            .AsNoTracking()
            .CountAsync(n => n.UserId == userId && !n.IsRead);

        return View(new NotificationBellViewModel
        {
            UnreadCount = unread,
            Variant = variant,
            RecentItems = recent
        });
    }
}

public class NotificationBellViewModel
{
    public int UnreadCount { get; set; }

    /// <summary>"light" cho header nền tối (Learner), "dark" cho header nền sáng (Admin/Moderator).</summary>
    public string Variant { get; set; } = "light";

    public List<NotificationPreviewItem> RecentItems { get; set; } = new();
}

public class NotificationPreviewItem
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string? Type { get; set; }
    public string? LinkUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
