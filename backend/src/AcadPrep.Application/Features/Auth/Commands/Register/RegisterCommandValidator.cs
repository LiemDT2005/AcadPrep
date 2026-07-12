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
            .NotEmpty().WithMessage("Vui lòng nhập email.")
            .EmailAddress().WithMessage("Địa chỉ email không hợp lệ.");

        // ── Password (BR-24) ─────────────────────────────────────────────────
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Vui lòng nhập mật khẩu.")
            .MinimumLength(8).WithMessage("Mật khẩu phải có ít nhất 8 ký tự.")
            .Must(p => PasswordRegex.IsMatch(p ?? string.Empty))
                .WithMessage("Mật khẩu phải chứa ít nhất một chữ hoa, một chữ thường, một chữ số và một ký tự đặc biệt.");

        // ── ConfirmPassword ──────────────────────────────────────────────────
        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Vui lòng xác nhận mật khẩu.")
            .Equal(x => x.Password)
                .WithMessage("Mật khẩu xác nhận không khớp. Vui lòng nhập lại.");

        // ── FullName ─────────────────────────────────────────────────────────
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Vui lòng nhập họ tên.")
            .MaximumLength(100).WithMessage("Họ tên không được vượt quá 100 ký tự.");
    }
}
