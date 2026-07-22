using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.Notifications.Commands.MarkRecentNotificationsRead;

/// <summary>
/// Đánh dấu đã đọc <paramref name="Count"/> thông báo gần nhất của người dùng hiện tại
/// (dùng khi mở popup chuông). Data trả về là số thông báo còn chưa đọc để cập nhật badge.
/// </summary>
public record MarkRecentNotificationsReadCommand(int Count = 5) : IRequest<Result<int>>;
