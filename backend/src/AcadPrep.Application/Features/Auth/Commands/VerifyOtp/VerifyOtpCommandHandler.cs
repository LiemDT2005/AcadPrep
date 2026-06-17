using Application.Common.Interfaces;
using Application.Features.Auth.DTOs;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Features.Auth.Commands.VerifyOtp;

public class VerifyOtpCommandHandler(
    IAppDbContext context,
    ICacheService cacheService
) : IRequestHandler<VerifyOtpCommand, VerifyOtpResultDto>
{
    private const int MaxFailedAttempts = 3;
    private const int LockoutMinutes = 15;
    private const int OtpExpirationMinutes = 5;

    public async Task<VerifyOtpResultDto> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
    {
        var cacheKey = $"register:staging:{request.Email}";
        var stagingData = await cacheService.GetAsync<StagingRegistrationData>(cacheKey, cancellationToken);

        if (stagingData == null)
        {
            return new VerifyOtpResultDto
            {
                Status = OtpVerificationStatus.OtpExpired,
                RemainingAttempts = 0
            };
        }

        if (stagingData.OtpCode != request.OtpCode)
        {
            stagingData.FailedAttempts++;
            await cacheService.SetAsync(cacheKey, stagingData, TimeSpan.FromMinutes(OtpExpirationMinutes), cancellationToken);

            if (stagingData.FailedAttempts >= MaxFailedAttempts)
            {
                var lockKey = $"register:resend-lock:{request.Email}";
                await cacheService.SetAsync(lockKey, true, TimeSpan.FromMinutes(LockoutMinutes), cancellationToken);
            }

            return new VerifyOtpResultDto
            {
                Status = OtpVerificationStatus.OtpMismatch,
                RemainingAttempts = MaxFailedAttempts - stagingData.FailedAttempts
            };
        }

        var user = new User
        {
            Email = stagingData.Email,
            PasswordHash = stagingData.PasswordHash,
            FullName = stagingData.FullName,
            RoleId = stagingData.RoleId,
            Status = UserStatus.Active
        };

        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        await cacheService.RemoveAsync(cacheKey, cancellationToken);

        return new VerifyOtpResultDto
        {
            Status = OtpVerificationStatus.Success,
            RemainingAttempts = 0
        };
    }
}
