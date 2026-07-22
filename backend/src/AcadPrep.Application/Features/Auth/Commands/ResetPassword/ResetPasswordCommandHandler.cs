using AcadPrep.Application.Common.Models;
using Application.Common.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.Commands.ResetPassword;

/// <summary>
/// Handler cho UC-8 Bước 2: Đặt lại mật khẩu.
/// BẮT BUỘC verify OTP từ cache "pwd-reset-otp:{email}" trước khi đổi password.
/// Sau khi đổi thành công: xóa OTP khỏi cache để không thể dùng lại.
/// Kiểm tra thêm user.Status == Active trước khi cho phép đổi password,
/// tránh trường hợp account bị khóa sau khi OTP đã gửi nhưng trước khi reset xong.
/// </summary>
internal sealed class ResetPasswordCommandHandler(
    IAppDbContext db,
    ICacheService cache,
    IPasswordHasher passwordHasher)
    : IRequestHandler<ResetPasswordCommand, Result<bool>>
{
    private const string PwdResetOtpPrefix = "pwd-reset-otp:";

    public async Task<Result<bool>> Handle(
        ResetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        // ── Bước 1: Đọc OTP từ cache — bảo vệ chống bypass ──────────────────
        // KHÔNG tin vào email param để đổi password ngay.
        // Phải verify OTP còn hợp lệ trong cache trước.
        var cacheKey = $"{PwdResetOtpPrefix}{request.Email}";
        var cachedEntry = await cache.GetAsync<PwdResetOtpEntry>(cacheKey, cancellationToken);

        // ── Bước 2: Kiểm tra OTP hợp lệ ─────────────────────────────────────
        // Không tồn tại trong cache → hết hạn hoặc chưa yêu cầu reset.
        // Không khớp OtpCode → nhập sai mã.
        if (cachedEntry is null || cachedEntry.OtpCode != request.OtpCode)
        {
            return Result<bool>.Failure(
                "The OTP code is invalid or has expired. Please request a new code.");
        }

        // ── Bước 3: Tìm user theo email (có tracking để EF ghi nhận thay đổi) ─
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user is null)
        {
            // Trường hợp bất thường: OTP tồn tại nhưng user không còn trong DB.
            return Result<bool>.Failure(
                "No account found associated with this email.");
        }

        // ── Bước 4: Kiểm tra user.Status == Active ────────────────────────────
        // Tránh trường hợp account bị Suspended sau khi OTP đã được gửi
        // nhưng trước khi user hoàn tất reset password.
        if (user.Status != UserStatus.Active)
        {
            return Result<bool>.Failure(
                "This account is not eligible for password reset.");
        }

        // ── Bước 5: Hash password mới qua IPasswordHasher (BCrypt work factor 12) ─
        var hashedPassword = passwordHasher.Hash(request.NewPassword);

        // ── Bước 6: Cập nhật password qua domain method ──────────────────────
        user.ChangePassword(hashedPassword);

        // ── Bước 7: Persist vào DB ────────────────────────────────────────────
        await db.SaveChangesAsync(cancellationToken);

        // ── Bước 8: Xóa OTP khỏi cache — không thể dùng lại ─────────────────
        await cache.RemoveAsync(cacheKey, cancellationToken);

        return Result<bool>.Success(true);
    }
}
