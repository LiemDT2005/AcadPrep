using Application.Features.Auth.DTOs;
using MediatR;

namespace Application.Features.Auth.Commands.ResendOtp;

public record ResendOtpCommand(string Email) : IRequest<ResendOtpResultDto>;
