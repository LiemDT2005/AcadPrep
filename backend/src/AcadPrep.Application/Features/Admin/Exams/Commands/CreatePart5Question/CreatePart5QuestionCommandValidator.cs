using FluentValidation;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.CreatePart5Question;

public class CreatePart5QuestionCommandValidator : AbstractValidator<CreatePart5QuestionCommand>
{
    public CreatePart5QuestionCommandValidator()
    {
        RuleFor(x => x.ExamId)
            .GreaterThan(0).WithMessage("Exam ID must be greater than 0.");

        RuleFor(x => x.Question)
            .NotNull().WithMessage("Question details are required.");

        When(x => x.Question != null, () =>
        {
            RuleFor(x => x.Question.QuestionNumber)
                .GreaterThan(0).WithMessage("Question number must be greater than 0.");

            RuleFor(x => x.Question.QuestionText)
                .NotEmpty().WithMessage("Question text cannot be empty.");

            RuleFor(x => x.Question.CorrectOption)
                .NotEmpty().WithMessage("Correct option is required.")
                .Must(x => x == "A" || x == "B" || x == "C" || x == "D")
                .WithMessage("Correct option must be A, B, C, or D.");

            RuleFor(x => x.Question.Options)
                .NotNull().WithMessage("Options list cannot be null.")
                .Must(o => o.Count == 4).WithMessage("Question must have exactly 4 options.");

            RuleForEach(x => x.Question.Options).ChildRules(option =>
            {
                option.RuleFor(o => o.Letter)
                    .NotEmpty().WithMessage("Option letter is required.")
                    .Must(o => o == "A" || o == "B" || o == "C" || o == "D")
                    .WithMessage("Option letter must be A, B, C, or D.");

                option.RuleFor(o => o.Text)
                    .NotEmpty().WithMessage("Option text cannot be empty.");
            });
        });
    }
}
