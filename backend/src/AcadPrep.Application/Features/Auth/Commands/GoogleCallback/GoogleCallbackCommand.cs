using AcadPrep.Application.Common.Models;
using Application.Features.Auth.Commands.Login;
using MediatR;

namespace Application.Features.Auth.Commands.GoogleCallback;

/// <summary>
/// Command cho UC-5.2: Login with Google.
/// Input đến từ ID Token đã được Google verify — KHÔNG qua FluentValidation.
/// </summary>
public record GoogleCallbackCommand(
    string Email,
    string GoogleId,
    string FullName) : IRequest<Result<LoginResultDto>>;
