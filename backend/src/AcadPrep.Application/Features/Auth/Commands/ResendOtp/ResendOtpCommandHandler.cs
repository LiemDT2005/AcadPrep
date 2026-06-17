using Application.Common.Interfaces;
using Application.Features.Auth.DTOs;
using MediatR;

namespace Application.Features.Auth.Commands.ResendOtp;

public class ResendOtpCommandHandler(
    ICacheService cacheService,
    IEmailService emailService
) : IRequestHandler<ResendOtpCommand, ResendOtpResultDto>
{
    private const int OtpExpirationMinutes = 5;

    public async Task<ResendOtpResultDto> Handle(ResendOtpCommand request, CancellationToken cancellationToken)
    {
        var lockKey = $"register:resend-lock:{request.Email}";
        var isLocked = await cacheService.GetAsync<bool>(lockKey, cancellationToken);

        if (isLocked)
        {
            return new ResendOtpResultDto
            {
                IsSuccess = false,
                IsLocked = true,
                IsExpired = false
            };
        }

        var stagingKey = $"register:staging:{request.Email}";
        var stagingData = await cacheService.GetAsync<StagingRegistrationData>(stagingKey, cancellationToken);

        if (stagingData == null)
        {
            return new ResendOtpResultDto
            {
                IsSuccess = false,
                IsLocked = false,
                IsExpired = true
            };
        }

        var newOtpCode = Random.Shared.Next(100000, 999999).ToString();
        stagingData.OtpCode = newOtpCode;
        stagingData.FailedAttempts = 0;

        await cacheService.SetAsync(stagingKey, stagingData, TimeSpan.FromMinutes(OtpExpirationMinutes), cancellationToken);

        await emailService.SendOtpAsync(request.Email, newOtpCode, cancellationToken);

        return new ResendOtpResultDto
        {
            IsSuccess = true,
            IsLocked = false,
            IsExpired = false
        };
    }
}
