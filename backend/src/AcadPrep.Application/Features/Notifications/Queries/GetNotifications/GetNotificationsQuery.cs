using AcadPrep.Application.Common.Models;
using AcadPrep.Application.Features.Notifications.DTOs;
using MediatR;

namespace AcadPrep.Application.Features.Notifications.Queries.GetNotifications;

public record GetNotificationsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    bool UnreadOnly = false
) : IRequest<Result<PaginatedList<NotificationDto>>>;
