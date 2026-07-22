using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.Notifications.Commands.MarkAllNotificationsRead;

public record MarkAllNotificationsReadCommand() : IRequest<Result>;
