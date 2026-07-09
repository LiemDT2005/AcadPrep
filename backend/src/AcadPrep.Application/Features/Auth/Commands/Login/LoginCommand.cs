using AcadPrep.Application.Common.Models;
using MediatR;

namespace Application.Features.Auth.Commands.Login;

/// <summary>
/// Command cho UC-5.1: đăng nhập bằng Email + Password.
/// Input đến từ form người dùng — bắt buộc qua FluentValidation pipeline.
/// </summary>
public record LoginCommand(string Email, string Password) : IRequest<Result<LoginResultDto>>;
