using AcadPrep.Application.Common.Models;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.Commands.Register;

/// <summary>
/// Handler cho UC-1: Đăng ký tài khoản bằng Email (Bước 1 — phát OTP).
/// Không ghi gì vào SQL Server ở bước này (BR-26 — chỉ Redis).
/// Tài khoản chỉ được tạo sau khi xác minh OTP thành công (UC-2).
/// </summary>
internal sealed class RegisterCommandHandler(
    IAppDbContext db,
    IPasswordHasher passwordHasher,
    IOtpIssuer otpIssuer)
    : IRequestHandler<RegisterCommand, Result<RegisterResultDto>>
{
    public async Task<Result<RegisterResultDto>> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        // ── Bước 1: Kiểm tra email đã tồn tại chưa (read-only, AsNoTracking) ──
        var emailExists = await db.Users
            .AsNoTracking()
            .AnyAsync(u => u.Email == request.Email, cancellationToken);

        if (emailExists)
        {
            return Result<RegisterResultDto>.Failure(
                "Email này đã được đăng ký. Vui lòng dùng email khác hoặc chọn Quên mật khẩu.");
        }

        // ── Bước 2: Hash password ─────────────────────────────────────────────
        var passwordHash = passwordHasher.Hash(request.Password);

        // ── Bước 3: Phát OTP (sinh, lưu Redis, gửi email) ────────────────────
        // Nếu email đang bị khóa (otp-lock:{email} còn hiệu lực), otpIssuer trả false.
        var issued = await otpIssuer.IssueOtpAsync(
            email:          request.Email,
            isReactivation: false,
            passwordHash:   passwordHash,
            fullName:       request.FullName,
            ct:             cancellationToken);

        if (!issued)
        {
            return Result<RegisterResultDto>.Failure(
                "Bạn đã nhập sai OTP quá số lần cho phép. Vui lòng thử lại sau 15 phút.");
        }

        // ── Bước 4: Trả kết quả — KHÔNG ghi SQL (BR-26) ──────────────────────
        return Result<RegisterResultDto>.Success(new RegisterResultDto(request.Email));
    }
}
