using AcadPrep.Application.Common.Models;
using AcadPrep.Application.Features.Notifications.Commands.MarkAllNotificationsRead;
using AcadPrep.Application.Features.Notifications.Commands.MarkNotificationRead;
using AcadPrep.Application.Features.Notifications.Commands.MarkRecentNotificationsRead;
using AcadPrep.Application.Features.Notifications.DTOs;
using AcadPrep.Application.Features.Notifications.Queries.GetNotifications;
using AcadPrep.Application.Features.Notifications.Queries.GetUnreadCount;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcadPrep.WebUI.Pages.Notifications;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IMediator _mediator;

    public IndexModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public PaginatedList<NotificationDto>? Notifications { get; set; }
    public int UnreadCount { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public bool UnreadOnly { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var list = await _mediator.Send(new GetNotificationsQuery(PageNumber, 10, UnreadOnly));
        if (list.IsSuccess)
            Notifications = list.Data;

        var count = await _mediator.Send(new GetUnreadCountQuery());
        if (count.IsSuccess)
            UnreadCount = count.Data;

        return Page();
    }

    // Mở một thông báo: đánh dấu đã đọc rồi điều hướng tới LinkUrl (nếu có).
    public async Task<IActionResult> OnPostOpenAsync(int id)
    {
        var result = await _mediator.Send(new MarkNotificationReadCommand(id));

        if (result.IsSuccess && !string.IsNullOrWhiteSpace(result.Data) && Url.IsLocalUrl(result.Data))
            return LocalRedirect(result.Data);

        return RedirectToPage(new { pageNumber = PageNumber, unreadOnly = UnreadOnly });
    }

    // Đánh dấu đã đọc tại chỗ (không điều hướng).
    public async Task<IActionResult> OnPostMarkReadAsync(int id)
    {
        await _mediator.Send(new MarkNotificationReadCommand(id));
        return RedirectToPage(new { pageNumber = PageNumber, unreadOnly = UnreadOnly });
    }

    public async Task<IActionResult> OnPostMarkAllReadAsync()
    {
        await _mediator.Send(new MarkAllNotificationsReadCommand());
        return RedirectToPage(new { pageNumber = PageNumber, unreadOnly = UnreadOnly });
    }

    // Gọi qua fetch khi mở popup chuông: đánh dấu các thông báo đang hiển thị là đã đọc.
    public async Task<IActionResult> OnPostMarkPreviewReadAsync(int count = 5)
    {
        var result = await _mediator.Send(new MarkRecentNotificationsReadCommand(count));
        var remaining = result.IsSuccess ? result.Data : 0;
        return new JsonResult(new { remaining });
    }
}
