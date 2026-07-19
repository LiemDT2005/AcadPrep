using FluentValidation;

namespace Application.Features.Auth.Commands.ForgotPassword;

/// <summary>
/// Validator cho ForgotPasswordCommand.
/// Chỉ validate format email — logic anti-enumeration nằm ở Handler.
/// </summary>
public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        // ── Email ────────────────────────────────────────────────────────────
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Vui lòng nhập địa chỉ email.")
            .EmailAddress().WithMessage("Địa chỉ email không hợp lệ.");
    }
}
