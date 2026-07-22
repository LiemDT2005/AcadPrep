using AcadPrep.Application.Common.Models;
using MediatR;

namespace Application.Features.Auth.Commands.VerifyOtp;

/// <summary>
/// Command cho UC-2: xác minh mã OTP 6 số.
/// Không có IsReactivation — Handler lấy từ OtpCacheEntry trong Redis.
/// </summary>
public record VerifyOtpCommand(string Email, string OtpCode)
    : IRequest<Result<VerifyOtpResultDto>>;
