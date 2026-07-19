using AcadPrep.Application.Common.Models;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Notifications.Commands.MarkRecentNotificationsRead;

public class MarkRecentNotificationsReadCommandHandler
    : IRequestHandler<MarkRecentNotificationsReadCommand, Result<int>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public MarkRecentNotificationsReadCommandHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<int>> Handle(MarkRecentNotificationsReadCommand request, CancellationToken cancellationToken)
    {
        if (!int.TryParse(_currentUser.UserId, out var userId))
            return Result<int>.Failure("Bạn cần đăng nhập để thực hiện thao tác này.");

        var count = request.Count < 1 ? 1 : request.Count;

        // Lấy đúng N thông báo gần nhất (khớp với những gì hiển thị trên popup) và đánh dấu đã đọc.
        var recentIds = await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(count)
            .Select(n => n.Id)
            .ToListAsync(cancellationToken);

        if (recentIds.Count > 0)
        {
            await _context.Notifications
                .Where(n => recentIds.Contains(n.Id) && !n.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), cancellationToken);
        }

        var remaining = await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead, cancellationToken);

        return Result<int>.Success(remaining);
    }
}
