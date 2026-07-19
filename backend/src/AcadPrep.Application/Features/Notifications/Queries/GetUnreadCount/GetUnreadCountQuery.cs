using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.Notifications.Queries.GetUnreadCount;

public record GetUnreadCountQuery() : IRequest<Result<int>>;
