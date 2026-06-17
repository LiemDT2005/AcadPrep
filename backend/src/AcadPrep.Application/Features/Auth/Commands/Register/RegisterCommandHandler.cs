using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Auth.DTOs;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler(
    IAppDbContext context,
    ICacheService cacheService,
    IEmailService emailService
) : IRequestHandler<RegisterCommand, RegisterResultDto>
{
    private const int OtpExpirationMinutes = 5;
    private const string DefaultRoleName = "User";

    public async Task<RegisterResultDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var emailExists = await context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Email == request.Email, cancellationToken);

        if (emailExists)
            throw new EmailAlreadyExistsException(request.Email);

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var otpCode = Random.Shared.Next(100000, 999999).ToString();

        var role = await context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.RoleName == DefaultRoleName, cancellationToken);

        var roleId = role?.RoleId ?? 1;

        var stagingData = new StagingRegistrationData
        {
            Email = request.Email,
            PasswordHash = passwordHash,
            FullName = request.FullName,
            RoleId = roleId,
            OtpCode = otpCode,
            FailedAttempts = 0
        };

        var cacheKey = $"register:staging:{request.Email}";
        await cacheService.SetAsync(cacheKey, stagingData, TimeSpan.FromMinutes(OtpExpirationMinutes), cancellationToken);

        await emailService.SendOtpAsync(request.Email, otpCode, cancellationToken);

        return new RegisterResultDto(request.Email);
    }
}
