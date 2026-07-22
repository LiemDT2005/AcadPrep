using FluentValidation;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.CreateTextCompletionSet;

public class CreateTextCompletionSetCommandValidator : AbstractValidator<CreateTextCompletionSetCommand>
{
    public CreateTextCompletionSetCommandValidator()
    {
        RuleFor(x => x.ExamId)
            .GreaterThan(0).WithMessage("Exam ID must be greater than 0.");

        RuleFor(x => x.Set)
            .NotNull().WithMessage("Set details are required.");

        When(x => x.Set != null, () =>
        {
            RuleFor(x => x.Set.Passage)
                .NotNull().WithMessage("Passage is required.");

            When(x => x.Set.Passage != null, () =>
            {
                RuleFor(x => x.Set.Passage.Explanation)
                    .NotEmpty().WithMessage("Passage explanation is required.");
            });
        });
    }
}
