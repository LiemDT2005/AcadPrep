using FluentValidation;

namespace AcadPrep.Application.Features.Practice.Commands.StartPractice;

public class StartPracticeCommandValidator : AbstractValidator<StartPracticeCommand>
{
    public StartPracticeCommandValidator()
    {
        RuleFor(v => v.ExamId)
            .GreaterThan(0).WithMessage("Invalid ExamId.");

        RuleFor(v => v.SelectedPartNumbers)
            .NotEmpty().WithMessage("Please select at least one Part to practice.");

        RuleFor(v => v.TimeLimitMinutes)
            .GreaterThan(0).When(v => v.TimeLimitMinutes.HasValue)
            .WithMessage("Practice time limit must be a positive number in minutes.");
    }
}
