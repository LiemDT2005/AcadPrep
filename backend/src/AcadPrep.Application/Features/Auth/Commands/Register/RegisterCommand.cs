using AcadPrep.Application.Common.Models;
using MediatR;

namespace Application.Features.Auth.Commands.Register;

/// <summary>
/// Command cho UC-1: Đăng ký tài khoản bằng Email + Password.
/// Input đến từ form người dùng — bắt buộc qua FluentValidation pipeline.
/// KHÔNG ghi SQL ở bước này — chỉ lưu OTP vào Redis (BR-26).
/// </summary>
public record RegisterCommand(
    string Email,
    string Password,
    string ConfirmPassword,
    string FullName) : IRequest<Result<RegisterResultDto>>;
