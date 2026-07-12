using AcadPrep.Application.Common.Models;
using MediatR;

namespace Application.Features.Auth.Commands.ResendOtp;

/// <summary>
/// Command cho UC-2: gửi lại OTP.
/// Không có IsReactivation — Handler lấy từ OtpCacheEntry hiện tại trong Redis.
/// </summary>
public record ResendOtpCommand(string Email)
    : IRequest<Result<ResendOtpResultDto>>;
