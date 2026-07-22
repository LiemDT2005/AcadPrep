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
    IPasswordHasher passwordHasher,
    IOtpIssuer otpIssuer)
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
            return Result<LoginResultDto>.Failure("Invalid email or password.");
        }

        // Bước 4b: PasswordHash null (tài khoản Google-only) hoặc verify sai → thất bại generic
        if (string.IsNullOrEmpty(user.PasswordHash) || !passwordHasher.Verify(user.PasswordHash, request.Password))
        {
            return Result<LoginResultDto>.Failure("Invalid email or password.");
        }

        // Bước 5: Tìm thấy + verify đúng nhưng tài khoản bị khóa
        if (user.Status == UserStatus.Suspended)
        {
            return Result<LoginResultDto>.Failure("Your account has been suspended. Please contact support.");
        }

        // Bước 6: Tài khoản chưa xác minh — phát OTP reactivation, trả Success với RequiresVerification = true
        if (user.Status == UserStatus.Inactive)
        {
            var issued = await otpIssuer.IssueOtpAsync(
                email:          user.Email,
                isReactivation: true,
                passwordHash:   null,
                fullName:       null,
                ct:             cancellationToken);

            if (!issued)
            {
                return Result<LoginResultDto>.Failure(
                    "You have entered the wrong OTP too many times. Please try again after 15 minutes.");
            }

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
