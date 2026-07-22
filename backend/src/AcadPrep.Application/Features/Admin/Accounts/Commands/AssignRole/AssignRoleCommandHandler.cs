using AcadPrep.Application.Common.Models;
using Application.Common.Interfaces;
using Domain.Constants;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Admin.Accounts.Commands.AssignRole;

public class AssignRoleCommandHandler : IRequestHandler<AssignRoleCommand, Result>
{
    private readonly IAppDbContext _context;
    private readonly INotificationService _notificationService;

    public AssignRoleCommandHandler(IAppDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<Result> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
            return Result.Failure("Account not found.");

        // Prevent modifying the Master Admin role
        var masterAdminId = await _context.Users
            .Where(u => u.Role.RoleName == nameof(UserRole.Admin))
            .OrderBy(u => u.Id)
            .Select(u => u.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (user.Id == masterAdminId)
            return Result.Failure("Cannot change the role of the Master Admin account.");

        // Prevent admin from changing their own role
        if (user.Id == request.CurrentAdminId)
            return Result.Failure("You cannot change your own role.");

        // Validate role exists
        var newRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.RoleId == request.NewRoleId, cancellationToken);

        if (newRole is null)
            return Result.Failure("Invalid role.");

        user.AssignRole(request.NewRoleId);
        await _context.SaveChangesAsync(cancellationToken);

        // Thông báo cho chủ tài khoản được đổi quyền (UC-15)
        await _notificationService.CreateAsync(
            userId: user.Id,
            title: "Account role updated",
            message: $"Your account has been granted the '{newRole.RoleName}' role. Some features may change based on the new role.",
            type: NotificationType.AccountRoleChanged,
            cancellationToken: cancellationToken);

        // Audit cho Admin: ghi nhận thay đổi phân quyền trong hệ thống (UC-15)
        await _notificationService.CreateForRoleAsync(
            roleName: nameof(UserRole.Admin),
            title: "Account role updated",
            message: $"Account '{user.FullName}' ({user.Email}) has been changed to the '{newRole.RoleName}' role.",
            type: NotificationType.AdminAccountRoleChanged,
            linkUrl: "/Admin/Accounts",
            cancellationToken: cancellationToken);

        return Result.Success();
    }
}
