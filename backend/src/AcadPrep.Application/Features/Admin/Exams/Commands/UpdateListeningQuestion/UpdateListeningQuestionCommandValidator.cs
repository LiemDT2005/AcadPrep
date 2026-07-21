using FluentValidation;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.UpdateListeningQuestion;

public class UpdateListeningQuestionCommandValidator : AbstractValidator<UpdateListeningQuestionCommand>
{
    public UpdateListeningQuestionCommandValidator()
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
