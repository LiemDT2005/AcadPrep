using AcadPrep.Application.Common.Models;
using Application.Common.Interfaces;
using Domain.Constants;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Admin.Accounts.Commands.UpdateAccountStatus;

public class UpdateAccountStatusCommandHandler : IRequestHandler<UpdateAccountStatusCommand, Result>
{
    private readonly IAppDbContext _context;
    private readonly INotificationService _notificationService;

    public UpdateAccountStatusCommandHandler(IAppDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<Result> Handle(UpdateAccountStatusCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
            return Result.Failure("Account not found.");

        // Prevent modifying the Master Admin
        var masterAdminId = await _context.Users
            .Where(u => u.Role.RoleName == nameof(UserRole.Admin))
            .OrderBy(u => u.Id)
            .Select(u => u.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (user.Id == masterAdminId)
            return Result.Failure("Cannot modify the Master Admin account status.");

        if (!Enum.TryParse<UserStatus>(request.NewStatus, true, out var newStatus))
            return Result.Failure($"Invalid status: {request.NewStatus}");

        if (newStatus == UserStatus.Active)
            user.Activate();
        else
        {
            // Use reflection to set Status since it's private set
            var statusProp = typeof(Domain.Entities.User).GetProperty("Status");
            statusProp?.SetValue(user, newStatus);
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Thông báo cho chủ tài khoản bị thay đổi trạng thái (UC-15)
        var isActivated = newStatus == UserStatus.Active;
        await _notificationService.CreateAsync(
            userId: user.Id,
            title: isActivated ? "Tài khoản đã được mở khóa" : "Tài khoản đã bị tạm khóa",
            message: isActivated
                ? "Tài khoản của bạn đã được kích hoạt lại. Bạn có thể tiếp tục sử dụng hệ thống bình thường."
                : "Tài khoản của bạn đã bị tạm khóa bởi quản trị viên. Vui lòng liên hệ hỗ trợ nếu cần trợ giúp.",
            type: NotificationType.AccountStatusChanged,
            cancellationToken: cancellationToken);

        return Result.Success();
    }
}
