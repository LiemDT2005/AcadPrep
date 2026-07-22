using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.Notifications.Commands.MarkNotificationRead;

/// <summary>
/// Đánh dấu một thông báo là đã đọc (UC-15.1). Data trả về là LinkUrl (nếu có)
/// để tầng trình bày điều hướng người dùng theo luồng "open + redirect".
/// </summary>
public record MarkNotificationReadCommand(int NotificationId) : IRequest<Result<string?>>;
