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
            .NotEmpty().WithMessage("Vui lòng nhập địa chỉ email.")
            .EmailAddress().WithMessage("Địa chỉ email không hợp lệ.");

        // ── OtpCode: đúng 6 chữ số ──────────────────────────────────────────
        RuleFor(x => x.OtpCode)
            .NotEmpty().WithMessage("Vui lòng nhập mã OTP.")
            .Matches(@"^\d{6}$").WithMessage("Mã OTP phải là 6 chữ số.");

        // ── NewPassword (BR-24) ──────────────────────────────────────────────
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Vui lòng nhập mật khẩu mới.")
            .MinimumLength(8).WithMessage("Mật khẩu phải có ít nhất 8 ký tự.")
            .Must(p => PasswordRegex.IsMatch(p ?? string.Empty))
                .WithMessage("Mật khẩu phải chứa ít nhất một chữ hoa, một chữ thường, một chữ số và một ký tự đặc biệt.");

        // ── ConfirmPassword ──────────────────────────────────────────────────
        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Vui lòng xác nhận mật khẩu mới.")
            .Equal(x => x.NewPassword)
                .WithMessage("Mật khẩu xác nhận không khớp. Vui lòng nhập lại.");
    }
}
