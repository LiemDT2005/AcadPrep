using FluentValidation;

namespace AcadPrep.Application.Features.Practice.Commands.StartPractice;

public class StartPracticeCommandValidator : AbstractValidator<StartPracticeCommand>
{
    public StartPracticeCommandValidator()
    {
        RuleFor(v => v.ExamId)
            .GreaterThan(0).WithMessage("ExamId không hợp lệ.");

        RuleFor(v => v.SelectedPartNumbers)
            .NotEmpty().WithMessage("Vui lòng chọn ít nhất một Part để luyện tập.");

        RuleFor(v => v.TimeLimitMinutes)
            .GreaterThan(0).When(v => v.TimeLimitMinutes.HasValue)
            .WithMessage("Giới hạn thời gian luyện tập phải là số phút dương.");
    }
}
