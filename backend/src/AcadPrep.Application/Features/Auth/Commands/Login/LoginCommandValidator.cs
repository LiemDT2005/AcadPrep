using FluentValidation;

namespace Application.Features.Auth.Commands.Login;

/// <summary>
/// Validator cho LoginCommand — chặn ở pipeline trước khi vào Handler.
/// Chỉ validate field rỗng/format cơ bản; logic nghiệp vụ nằm trong Handler.
/// </summary>
public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Vui lòng nhập email.")
            .EmailAddress().WithMessage("Email không hợp lệ.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Vui lòng nhập mật khẩu.");
    }
}
