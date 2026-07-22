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

        // Áp dụng trạng thái mới thông qua hàm ChangeStatus (đã chứa rule: Suspended -> Inactive)
        user.ChangeStatus(newStatus);

        await _context.SaveChangesAsync(cancellationToken);

        // Thông báo cho chủ tài khoản bị thay đổi trạng thái (UC-15)
        var isActivatedOrInactive = user.Status == UserStatus.Active || user.Status == UserStatus.Inactive;
        await _notificationService.CreateAsync(
            userId: user.Id,
            title: isActivatedOrInactive ? "Account unlocked" : "Account suspended",
            message: isActivatedOrInactive
                ? "Your account has been unlocked by an administrator. Please log in to continue."
                : "Your account has been temporarily suspended by an administrator. Please contact support if you need help.",
            type: NotificationType.AccountStatusChanged,
            cancellationToken: cancellationToken);

        return Result.Success();
    }
}
