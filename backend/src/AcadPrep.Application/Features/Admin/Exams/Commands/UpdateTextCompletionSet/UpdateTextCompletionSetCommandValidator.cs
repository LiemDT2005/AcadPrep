using FluentValidation;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.UpdateTextCompletionSet;

public class UpdateTextCompletionSetCommandValidator : AbstractValidator<UpdateTextCompletionSetCommand>
{
    public UpdateTextCompletionSetCommandValidator()
    {
        RuleFor(x => x.ExamId).GreaterThan(0);
        RuleFor(x => x.PassageId).GreaterThan(0);
        RuleFor(x => x.Set).NotNull();

        When(x => x.Set?.Passage != null, () =>
        {
            RuleFor(x => x.Set.Passage.Explanation)
                .NotEmpty().WithMessage("Passage explanation is required.");
        });
    }
}
