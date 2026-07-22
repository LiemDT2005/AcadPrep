using FluentValidation;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.UpdatePart5Question;

public class UpdatePart5QuestionCommandValidator : AbstractValidator<UpdatePart5QuestionCommand>
{
    public UpdatePart5QuestionCommandValidator()
    {
        RuleFor(x => x.ExamId).GreaterThan(0);
        RuleFor(x => x.QuestionId).GreaterThan(0);
        RuleFor(x => x.Question).NotNull();

        When(x => x.Question != null, () =>
        {
            RuleFor(x => x.Question.Explanation)
                .NotEmpty().WithMessage("Explanation is required.");
        });
    }
}
