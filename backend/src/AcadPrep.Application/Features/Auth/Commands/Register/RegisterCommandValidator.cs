using FluentValidation;

namespace Application.Features.Auth.Commands.Register;

/// <summary>
/// Validator cho RegisterCommand — chặn ở pipeline trước khi vào Handler.
/// Tất cả message bằng tiếng Việt theo convention của team.
/// </summary>
public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    // Regex BR-24: tối thiểu 8 ký tự, có chữ hoa, chữ thường, số và ký tự đặc biệt.
    private static readonly System.Text.RegularExpressions.Regex PasswordRegex =
        new(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,}$",
            System.Text.RegularExpressions.RegexOptions.Compiled);

    public RegisterCommandValidator()
    {
        // ── Email ────────────────────────────────────────────────────────────
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Please enter your email.")
            .EmailAddress().WithMessage("Invalid email format.");

        // ── Password (BR-24) ─────────────────────────────────────────────────
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Please enter your password.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .Must(p => PasswordRegex.IsMatch(p ?? string.Empty))
                .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.");

        // ── ConfirmPassword ──────────────────────────────────────────────────
        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Please confirm your password.")
            .Equal(x => x.Password)
                .WithMessage("Passwords do not match. Please try again.");

        // ── FullName ─────────────────────────────────────────────────────────
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Please enter your full name.")
            .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters.");
    }
}
