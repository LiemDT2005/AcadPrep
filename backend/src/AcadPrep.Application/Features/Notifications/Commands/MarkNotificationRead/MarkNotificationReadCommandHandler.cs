using AcadPrep.Application.Common.Models;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Notifications.Commands.MarkNotificationRead;

public class MarkNotificationReadCommandHandler
    : IRequestHandler<MarkNotificationReadCommand, Result<string?>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public MarkNotificationReadCommandHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<string?>> Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        if (!int.TryParse(_currentUser.UserId, out var userId))
            return Result<string?>.Failure("Bạn cần đăng nhập để thực hiện thao tác này.");

        // Ràng buộc ownership: chỉ đọc/sửa thông báo thuộc về chính người dùng hiện tại.
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == request.NotificationId && n.UserId == userId, cancellationToken);

        if (notification is null)
            return Result<string?>.Failure("Không tìm thấy thông báo.");

        // Idempotent: đã đọc rồi thì không cần ghi DB.
        if (!notification.IsRead)
        {
            notification.IsRead = true;
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Result<string?>.Success(notification.LinkUrl);
    }
}
