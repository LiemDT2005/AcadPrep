using Application.Common.Interfaces;
using AcadPrep.Application.Common.Models;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.Commands.Login;

/// <summary>
/// Handler cho UC-5.1: Login with Email.
/// KHÔNG phát cookie — chỉ trả Result&lt;LoginResultDto&gt;.
/// PageModel đảm nhận HttpContext.SignInAsync sau khi nhận Success.
/// </summary>
internal sealed class LoginCommandHandler(
    IAppDbContext db,
    IPasswordHasher passwordHasher)
    : IRequestHandler<LoginCommand, Result<LoginResultDto>>
{
    public async Task<Result<LoginResultDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Bước 3: Tìm User theo email — read-only, không ghi gì sau đó
        var user = await db.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        // Bước 4a: Không tìm thấy email → thất bại generic (không tiết lộ email tồn tại hay không)
        if (user is null)
        {
            return Result<LoginResultDto>.Failure("Email hoặc mật khẩu không chính xác.");
        }

        // Bước 4b: PasswordHash null (tài khoản Google-only) hoặc verify sai → thất bại generic
        if (string.IsNullOrEmpty(user.PasswordHash) || !passwordHasher.Verify(user.PasswordHash, request.Password))
        {
            return Result<LoginResultDto>.Failure("Email hoặc mật khẩu không chính xác.");
        }

        // Bước 5: Tìm thấy + verify đúng nhưng tài khoản bị khóa
        if (user.Status == UserStatus.Suspended)
        {
            return Result<LoginResultDto>.Failure("Tài khoản của bạn đã bị khóa. Vui lòng liên hệ hỗ trợ.");
        }

        // Bước 6: Tài khoản chưa xác minh — trả Success nhưng RequiresVerification = true
        // PageModel sẽ hiển thị thông báo, KHÔNG phát cookie
        if (user.Status == UserStatus.Inactive)
        {
            return Result<LoginResultDto>.Success(new LoginResultDto(
                UserId: user.Id,
                Email: user.Email,
                FullName: user.FullName,
                Role: user.Role.RoleName,
                RequiresVerification: true));
        }

        // Bước 7: Status == Active và password đúng → đăng nhập thành công
        return Result<LoginResultDto>.Success(new LoginResultDto(
            UserId: user.Id,
            Email: user.Email,
            FullName: user.FullName,
            Role: user.Role.RoleName,
            RequiresVerification: false));
    }
}
