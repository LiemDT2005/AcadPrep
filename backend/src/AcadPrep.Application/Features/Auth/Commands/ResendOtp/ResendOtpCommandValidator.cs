using FluentValidation;

namespace Application.Features.Auth.Commands.ResendOtp;

/// <summary>
/// Validator cho ResendOtpCommand — bắt buộc tạo dù Command chỉ có 1 field,
/// để đồng nhất convention: mọi Command đều qua FluentValidation pipeline.
/// </summary>
public sealed class ResendOtpCommandValidator : AbstractValidator<ResendOtpCommand>
{
    public ResendOtpCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email cannot be empty.")
            .EmailAddress().WithMessage("Invalid email format.");
    }
}
