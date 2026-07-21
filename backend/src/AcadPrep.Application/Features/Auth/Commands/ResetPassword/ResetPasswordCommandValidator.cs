using FluentValidation;

namespace Application.Features.Auth.Commands.ResetPassword;

/// <summary>
/// Validator cho ResetPasswordCommand.
/// Validate tất cả field trước khi Handler xử lý.
/// Message tiếng Việt theo convention của team.
/// </summary>
public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    // Regex BR-24: tối thiểu 8 ký tự, có chữ hoa, chữ thường, số và ký tự đặc biệt.
    private static readonly System.Text.RegularExpressions.Regex PasswordRegex =
        new(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,}$",
            System.Text.RegularExpressions.RegexOptions.Compiled);

    public ResetPasswordCommandValidator()
    {
        // ── Email ────────────────────────────────────────────────────────────
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Please enter your email address.")
            .EmailAddress().WithMessage("Invalid email format.");

        // ── OtpCode: đúng 6 chữ số ──────────────────────────────────────────
        RuleFor(x => x.OtpCode)
            .NotEmpty().WithMessage("Please enter the OTP code.")
            .Matches(@"^\d{6}$").WithMessage("The OTP code must be 6 digits.");

        // ── NewPassword (BR-24) ──────────────────────────────────────────────
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Please enter a new password.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .Must(p => PasswordRegex.IsMatch(p ?? string.Empty))
                .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.");

        // ── ConfirmPassword ──────────────────────────────────────────────────
        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Please confirm your new password.")
            .Equal(x => x.NewPassword)
                .WithMessage("Passwords do not match. Please try again.");
    }
}
