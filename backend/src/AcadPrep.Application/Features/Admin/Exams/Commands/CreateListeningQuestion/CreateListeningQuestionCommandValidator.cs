using FluentValidation;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.CreateListeningQuestion;

public class CreateListeningQuestionCommandValidator : AbstractValidator<CreateListeningQuestionCommand>
{
    public CreateListeningQuestionCommandValidator()
    {
        RuleFor(x => x.ExamId)
            .GreaterThan(0).WithMessage("Exam ID must be greater than 0.");

        RuleFor(x => x.Part)
            .InclusiveBetween(1, 2).WithMessage("Part must be 1 or 2.");

        RuleFor(x => x.Question)
            .NotNull().WithMessage("Question details are required.");

        When(x => x.Question != null, () =>
        {
            RuleFor(x => x.Question.QuestionNumber)
                .GreaterThan(0).WithMessage("Question number must be greater than 0.");

            RuleFor(x => x.Question.Explanation)
                .NotEmpty().WithMessage("Explanation is required.");

            // Part 2 only has 3 options (A, B, C); Part 1 has 4 (A, B, C, D)
            When(x => x.Part == 2, () =>
            {
                RuleFor(x => x.Question.CorrectOption)
                    .NotEmpty().WithMessage("Correct option is required.")
                    .Must(x => x == "A" || x == "B" || x == "C")
                    .WithMessage("Correct option must be A, B, or C for Part 2.");

                RuleFor(x => x.Question.Options)
                    .NotNull().WithMessage("Options list cannot be null.")
                    .Must(o => o.Count == 3).WithMessage("Part 2 question must have exactly 3 options.");

                RuleForEach(x => x.Question.Options).ChildRules(option =>
                {
                    option.RuleFor(o => o.Letter)
                        .NotEmpty().WithMessage("Option letter is required.")
                        .Must(o => o == "A" || o == "B" || o == "C")
                        .WithMessage("Option letter must be A, B, or C for Part 2.");

                    option.RuleFor(o => o.Text)
                        .NotEmpty().WithMessage("Option text cannot be empty.");
                });
            }).Otherwise(() =>
            {
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
        });
    }
}
