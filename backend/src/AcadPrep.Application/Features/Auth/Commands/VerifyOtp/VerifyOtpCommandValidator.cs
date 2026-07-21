using FluentValidation;

namespace Application.Features.Auth.Commands.VerifyOtp;

/// <summary>
/// Validator cho VerifyOtpCommand — chặn ở pipeline trước khi vào Handler.
/// </summary>
public sealed class VerifyOtpCommandValidator : AbstractValidator<VerifyOtpCommand>
{
    public VerifyOtpCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email cannot be empty.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.OtpCode)
            .NotEmpty().WithMessage("Please enter the OTP code.")
            .Matches(@"^\d{6}$").WithMessage("The OTP code must be exactly 6 digits.");
    }
}
