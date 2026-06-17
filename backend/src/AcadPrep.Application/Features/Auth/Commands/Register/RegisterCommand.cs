using Application.Features.Auth.DTOs;
using MediatR;

namespace Application.Features.Auth.Commands.Register;

public record RegisterCommand(
    string Email,
    string Password,
    string ConfirmPassword,
    string FullName
) : IRequest<RegisterResultDto>;
