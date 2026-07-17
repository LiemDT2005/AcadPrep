using Application.Common.Constants;
using Application.Common.Interfaces;
using AcadPrep.Application.Common.Models;
using Application.Features.Auth.Commands.Login;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.Commands.GoogleCallback;

/// <summary>
/// Handler cho UC-5.2: Login with Google.
/// KHÔNG phát cookie — chỉ trả Result&lt;LoginResultDto&gt;.
/// PageModel đảm nhận HttpContext.SignInAsync sau khi nhận Success.
/// </summary>
internal sealed class GoogleCallbackCommandHandler(
    IAppDbContext db,
    TimeProvider timeProvider)
    : IRequestHandler<GoogleCallbackCommand, Result<LoginResultDto>>
{

    public async Task<Result<LoginResultDto>> Handle(GoogleCallbackCommand request, CancellationToken cancellationToken)
    {
        // Bước 2: Tìm User theo email — CÓ tracking vì có khả năng ghi (Activate / LinkGoogleId)
        var user = await db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user is not null)
        {
            // Bước 3: Tìm thấy nhưng tài khoản bị khóa → từ chối, không ghi gì
            if (user.Status == UserStatus.Suspended)
            {
                return Result<LoginResultDto>.Failure("Tài khoản của bạn đã bị khóa. Vui lòng liên hệ hỗ trợ.");
            }

            // Bước 4: Status != Suspended (Active hoặc Inactive) — gộp chung vì Google đã verify email
            // 4a: Kích hoạt tài khoản nếu đang Inactive (idempotent nếu đã Active)
            user.Activate();

            // 4b: Liên kết GoogleId nếu chưa có (idempotent nếu đã có)
            user.LinkGoogleIdentity(request.GoogleId);

            // SaveChangesAsync một lần duy nhất cho cả 2 thay đổi trên cùng 1 entity tracked
            await db.SaveChangesAsync(cancellationToken);

            return Result<LoginResultDto>.Success(new LoginResultDto(
                UserId: user.Id,
                Email: user.Email,
                FullName: user.FullName,
                Role: user.Role.RoleName));
        }

        // Bước 5: Không tìm thấy user → tạo mới (Status = Active, GoogleId set sẵn, không có PasswordHash)
        var defaultRole = await db.Roles
            .FirstOrDefaultAsync(r => r.RoleId == RoleConstants.DefaultUserRoleId, cancellationToken);

        var roleName = defaultRole?.RoleName ?? nameof(UserRole.Learner);

        var newUser = User.CreateFromGoogle(
            email:          request.Email,
            fullName:       request.FullName,
            googleId:       request.GoogleId,
            defaultRoleId:  RoleConstants.DefaultUserRoleId,
            createdAt:      timeProvider.GetUtcNow().UtcDateTime);

        db.Users.Add(newUser);

        // SaveChangesAsync một lần duy nhất
        await db.SaveChangesAsync(cancellationToken);

        return Result<LoginResultDto>.Success(new LoginResultDto(
            UserId: newUser.Id,
            Email: newUser.Email,
            FullName: newUser.FullName,
            Role: roleName));
    }
}
