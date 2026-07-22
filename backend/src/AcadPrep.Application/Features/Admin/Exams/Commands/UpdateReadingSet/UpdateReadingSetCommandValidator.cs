using FluentValidation;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.UpdateReadingSet;

public class UpdateReadingSetCommandValidator : AbstractValidator<UpdateReadingSetCommand>
{
    public UpdateReadingSetCommandValidator()
    {
        RuleFor(x => x.ExamId).GreaterThan(0);
        RuleFor(x => x.QuestionGroupId).GreaterThan(0);
        RuleFor(x => x.Set).NotNull();

        When(x => x.Set != null, () =>
        {
            RuleFor(x => x.Set.Name).NotEmpty().WithMessage("Reading set name is required.");
            RuleFor(x => x.Set.Explanation).NotEmpty().WithMessage("Set explanation is required.");
        });
    }
}
