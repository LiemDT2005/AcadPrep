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
            .NotEmpty().WithMessage("Please enter your email.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Please enter your password.");
    }
}
