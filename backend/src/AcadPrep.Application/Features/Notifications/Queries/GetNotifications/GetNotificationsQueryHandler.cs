using AcadPrep.Application.Common.Models;
using AcadPrep.Application.Features.Notifications.DTOs;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Notifications.Queries.GetNotifications;

public class GetNotificationsQueryHandler
    : IRequestHandler<GetNotificationsQuery, Result<PaginatedList<NotificationDto>>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetNotificationsQueryHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<PaginatedList<NotificationDto>>> Handle(
        GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        if (!int.TryParse(_currentUser.UserId, out var userId))
            return Result<PaginatedList<NotificationDto>>.Failure("Bạn cần đăng nhập để xem thông báo.");

        var query = _context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId);

        if (request.UnreadOnly)
            query = query.Where(n => !n.IsRead);

        var projected = query
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                LinkUrl = n.LinkUrl,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            });

        var result = await PaginatedList<NotificationDto>.CreateAsync(
            projected, request.PageNumber, request.PageSize);

        return Result<PaginatedList<NotificationDto>>.Success(result);
    }
}
