using FluentValidation;

namespace AcadPrep.Application.Features.AiQna.Commands.AskAi;

public class AskAiCommandValidator : AbstractValidator<AskAiCommand>
{
    public AskAiCommandValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Nội dung tin nhắn không được để trống.")
            .MaximumLength(1000).WithMessage("Nội dung tin nhắn không được vượt quá 1000 ký tự.");

        RuleFor(x => x.History)
            .Must(history => history == null || history.Count <= 10)
            .WithMessage("Lịch sử trò chuyện không được vượt quá 10 tin nhắn.");
    }
}
