using AcadPrep.Application.Common.Models;
using Application.Common.Constants;
using Application.Common.Interfaces;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
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
    INotificationService notificationService,
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
                "The OTP code has expired. Please click 'Resend' to get a new code.");
        }

        // ── Bước 2b: Kiểm tra hết hạn TRƯỚC khi so sánh OtpCode ─────────────
        // Đảm bảo OTP hết hạn không bao giờ bị tính vào FailedAttempts hay gây lock oan.
        if (timeProvider.GetUtcNow().UtcDateTime >= entry.ExpiresAtUtc)
        {
            await cache.RemoveAsync(otpKey, cancellationToken);
            return Result<VerifyOtpResultDto>.Failure(
                "The OTP code has expired. Please click 'Resend' to get a new code.");
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
                    "The OTP code is incorrect. You have entered incorrectly more than 3 times, please try again after 15 minutes.");
            }

            // Tính remaining TTL — không reset về 5 phút (hết hạn đã bị chặn ở bước 2b)
            var remaining = entry.ExpiresAtUtc - timeProvider.GetUtcNow().UtcDateTime;
            await cache.SetAsync(otpKey, entry, remaining, cancellationToken);

            return Result<VerifyOtpResultDto>.Failure(
                "The OTP code is incorrect. Please check your email again.");
        }

        // ── Bước 4a/4b: Tạo hoặc kích hoạt User ─────────────────────────────
        User activatedUser;
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
            activatedUser = newUser;
        }
        else
        {
            // 4b: Tái kích hoạt — CÓ tracking vì sẽ UPDATE
            var existingUser = await db.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            if (existingUser is not null)
            {
                existingUser.Activate();
                activatedUser = existingUser;
            }
            else
            {
                return Result<VerifyOtpResultDto>.Failure(
                    "Account not found. Please try registering again.");
            }
        }

        // ── Bước 4c: SaveChangesAsync — một lần duy nhất ─────────────────────
        await db.SaveChangesAsync(cancellationToken);

        // ── Bước 4d: Xoá OTP khỏi cache ──────────────────────────────────────
        await cache.RemoveAsync(otpKey, cancellationToken);

        // ── Bước 4e: Gửi thông báo chào mừng (UC-15) ─────────────────────────
        var isReactivation = entry.IsReactivation;
        await notificationService.CreateAsync(
            userId: activatedUser.Id,
            title: isReactivation ? "Welcome back!" : "Welcome to AcadPrep!",
            message: isReactivation
                ? "Your account has been reactivated. Continue your journey to conquer TOEIC today."
                : "Your account has been successfully activated. Start your journey to conquer TOEIC today.",
            type: NotificationType.AccountWelcome,
            linkUrl: "/Account/Profile",
            cancellationToken: cancellationToken);

        // ── Bước 4f: Cảnh báo cho Admin khi có người dùng mới đăng ký ─────────
        if (!isReactivation)
        {
            await notificationService.CreateForRoleAsync(
                roleName: nameof(UserRole.Admin),
                title: "New User Registered",
                message: $"New account '{activatedUser.FullName}' ({activatedUser.Email}) has just been successfully activated.",
                type: NotificationType.AdminNewUserRegistered,
                linkUrl: "/Admin/Accounts",
                cancellationToken: cancellationToken);
        }

        // ── Bước 4g: Trả kết quả ─────────────────────────────────────────────
        return Result<VerifyOtpResultDto>.Success(new VerifyOtpResultDto(request.Email));
    }
}
