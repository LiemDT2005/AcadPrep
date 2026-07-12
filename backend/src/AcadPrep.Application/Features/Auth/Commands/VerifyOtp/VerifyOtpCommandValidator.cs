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
            .NotEmpty().WithMessage("Email không được để trống.")
            .EmailAddress().WithMessage("Địa chỉ email không hợp lệ.");

        RuleFor(x => x.OtpCode)
            .NotEmpty().WithMessage("Vui lòng nhập mã OTP.")
            .Matches(@"^\d{6}$").WithMessage("Mã OTP phải gồm đúng 6 chữ số.");
    }
}
