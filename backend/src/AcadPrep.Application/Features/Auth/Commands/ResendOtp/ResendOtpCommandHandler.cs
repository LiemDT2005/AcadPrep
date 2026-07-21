using AcadPrep.Application.Common.Models;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Auth.Commands.ResendOtp;

/// <summary>
/// Handler cho UC-2: gửi lại OTP.
/// Tái sử dụng IOtpIssuer (đã check lock, sinh OTP mới, overwrite cache, gửi email).
/// Không tự viết lại logic sinh OTP/lưu cache/gửi email.
/// </summary>
internal sealed class ResendOtpCommandHandler(
    ICacheService cache,
    IOtpIssuer otpIssuer)
    : IRequestHandler<ResendOtpCommand, Result<ResendOtpResultDto>>
{
    private const string OtpKeyPrefix   = "otp:";
    private const string LockKeyPrefix  = "otp-lock:";

    public async Task<Result<ResendOtpResultDto>> Handle(
        ResendOtpCommand request,
        CancellationToken cancellationToken)
    {
        // ── Bước 1: Check lock trước — nếu locked, otp:key đã bị xóa ────────
        var lockKey   = $"{LockKeyPrefix}{request.Email}";
        var isLocked = await cache.GetAsync<bool>(lockKey, cancellationToken);
        if (isLocked)
        {
            return Result<ResendOtpResultDto>.Failure(
                "You have entered the wrong OTP too many times. Please try again after 15 minutes.");
        }

        // ── Bước 2: Lấy OtpCacheEntry hiện tại ──────────────────────────────
        var otpKey = $"{OtpKeyPrefix}{request.Email}";
        var entry  = await cache.GetAsync<OtpCacheEntry>(otpKey, cancellationToken);

        if (entry is null)
        {
            return Result<ResendOtpResultDto>.Failure(
                "The authentication session has expired. Please start over.");
        }

        // ── Bước 3: Gọi IOtpIssuer — tái sử dụng Batch A ────────────────────
        var issued = await otpIssuer.IssueOtpAsync(
            email:          entry.Email,
            isReactivation: entry.IsReactivation,
            passwordHash:   entry.PasswordHash,
            fullName:       entry.FullName,
            ct:             cancellationToken);

        if (!issued)
        {
            return Result<ResendOtpResultDto>.Failure(
                "You have entered the wrong OTP too many times. Please try again after 15 minutes.");
        }

        // ── Bước 4: Gửi lại thành công ───────────────────────────────────────
        return Result<ResendOtpResultDto>.Success(new ResendOtpResultDto(request.Email));
    }
}
