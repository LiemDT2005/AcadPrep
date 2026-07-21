using AcadPrep.Application.Common.Models;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Auth.Commands.ResetPassword;

/// <summary>
/// Command cho UC-8: Quên mật khẩu — Bước 2 (Đặt lại mật khẩu).
/// Yêu cầu OTP hợp lệ được gửi ở Bước 1 (ForgotPasswordCommand).
/// OTP được verify từ cache key "pwd-reset-otp:{email}" trước khi cho phép đổi password.
/// </summary>
public record ResetPasswordCommand(
    string Email,
    string OtpCode,
    string NewPassword,
    string ConfirmPassword) : IRequest<Result<bool>>;
