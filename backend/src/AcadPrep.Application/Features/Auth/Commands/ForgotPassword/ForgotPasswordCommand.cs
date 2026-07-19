using AcadPrep.Application.Common.Models;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Auth.Commands.ForgotPassword;

/// <summary>
/// Command cho UC-8: Quên mật khẩu — Bước 1.
/// Gửi OTP về email để xác thực quyền sở hữu trước khi cho phép đặt lại mật khẩu.
/// Anti-enumeration: luôn trả Success bất kể email có tồn tại trong hệ thống hay không.
/// </summary>
public record ForgotPasswordCommand(string Email) : IRequest<Result<ForgotPasswordResultDto>>;
