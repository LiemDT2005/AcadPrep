using Application.Features.Auth.DTOs;
using MediatR;

namespace Application.Features.Auth.Commands.VerifyOtp;

public record VerifyOtpCommand(string Email, string OtpCode) : IRequest<VerifyOtpResultDto>;
