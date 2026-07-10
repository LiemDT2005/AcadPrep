using AcadPrep.Application.Common.Models;
using Application.Common.Constants;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.Commands.VerifyOtp;

/// <summary>
/// Handler cho UC-2: xác minh OTP — tạo/kích hoạt User, xoá cache.
/// Không phát cookie — PageModel đảm nhận sau khi nhận Success.
/// </summary>
internal sealed class VerifyOtpCommandHandler(
    IAppDbContext db,
    ICacheService cache,
    TimeProvider timeProvider)
    : IRequestHandler<VerifyOtpCommand, Result<VerifyOtpResultDto>>
{
    private const string OtpKeyPrefix  = "otp:";
    private const string LockKeyPrefix = "otp-lock:";

    public async Task<Result<VerifyOtpResultDto>> Handle(
        VerifyOtpCommand request,
        CancellationToken cancellationToken)
    {
        var otpKey  = $"{OtpKeyPrefix}{request.Email}";
        var lockKey = $"{LockKeyPrefix}{request.Email}";

        // ── Bước 1: Lấy OtpCacheEntry ────────────────────────────────────────
        var entry = await cache.GetAsync<OtpCacheEntry>(otpKey, cancellationToken);

        // ── Bước 2: Không tìm thấy = hết hạn ────────────────────────────────
        if (entry is null)
        {
            return Result<VerifyOtpResultDto>.Failure(
                "Mã OTP đã hết hạn. Vui lòng bấm 'Gửi lại' để nhận mã mới.");
        }

        // ── Bước 2b: Kiểm tra hết hạn TRƯỚC khi so sánh OtpCode ─────────────
        // Đảm bảo OTP hết hạn không bao giờ bị tính vào FailedAttempts hay gây lock oan.
        if (timeProvider.GetUtcNow().UtcDateTime >= entry.ExpiresAtUtc)
        {
            await cache.RemoveAsync(otpKey, cancellationToken);
            return Result<VerifyOtpResultDto>.Failure(
                "Mã OTP đã hết hạn. Vui lòng bấm 'Gửi lại' để nhận mã mới.");
        }

        // ── Bước 3: OTP sai ──────────────────────────────────────────────────
        if (entry.OtpCode != request.OtpCode)
        {
            entry = entry with { FailedAttempts = entry.FailedAttempts + 1 };

            // BR1: sai >= 3 lần → khóa 15 phút
            if (entry.FailedAttempts >= 3)
            {
                await cache.RemoveAsync(otpKey, cancellationToken);
                await cache.SetAsync(lockKey, true, TimeSpan.FromMinutes(15), cancellationToken);

                return Result<VerifyOtpResultDto>.Failure(
                    "Mã OTP không chính xác. Bạn đã nhập sai quá 3 lần, vui lòng thử lại sau 15 phút.");
            }

            // Tính remaining TTL — không reset về 5 phút (hết hạn đã bị chặn ở bước 2b)
            var remaining = entry.ExpiresAtUtc - timeProvider.GetUtcNow().UtcDateTime;
            await cache.SetAsync(otpKey, entry, remaining, cancellationToken);

            return Result<VerifyOtpResultDto>.Failure(
                "Mã OTP không chính xác. Vui lòng kiểm tra lại email.");
        }

        // ── Bước 4a/4b: Tạo hoặc kích hoạt User ─────────────────────────────
        if (!entry.IsReactivation)
        {
            // 4a: Đăng ký mới — User.Create → Status = Inactive → Activate
            var newUser = User.Create(
                email:        request.Email,
                fullName:     entry.FullName!,
                passwordHash: entry.PasswordHash!,
                roleId:       RoleConstants.DefaultUserRoleId,
                createdAt:    timeProvider.GetUtcNow().UtcDateTime);

            newUser.Activate();
            db.Users.Add(newUser);
        }
        else
        {
            // 4b: Tái kích hoạt — CÓ tracking vì sẽ UPDATE
            var existingUser = await db.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            if (existingUser is not null)
            {
                existingUser.Activate();
            }
            else
            {
                return Result<VerifyOtpResultDto>.Failure(
                    "Không tìm thấy tài khoản tương ứng. Vui lòng thử đăng ký lại.");
            }
        }

        // ── Bước 4c: SaveChangesAsync — một lần duy nhất ─────────────────────
        await db.SaveChangesAsync(cancellationToken);

        // ── Bước 4d: Xoá OTP khỏi cache ──────────────────────────────────────
        await cache.RemoveAsync(otpKey, cancellationToken);

        // ── Bước 4e: Trả kết quả ─────────────────────────────────────────────
        return Result<VerifyOtpResultDto>.Success(new VerifyOtpResultDto(request.Email));
    }
}
