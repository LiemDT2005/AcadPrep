using FluentValidation;

namespace Application.Features.Exams.Commands.CreateExam;

public class CreateExamCommandValidator : AbstractValidator<CreateExamCommand>
{
    public CreateExamCommandValidator()
    {
        RuleFor(x => x.CreateExamDto)
            .NotNull().WithMessage("Exam information is required.");

        When(x => x.CreateExamDto != null, () =>
        {
            RuleFor(x => x.CreateExamDto.Title)
                .NotEmpty().WithMessage("Exam title cannot be empty.")
                .MaximumLength(255).WithMessage("Exam title cannot exceed 255 characters.");

            RuleFor(x => x.CreateExamDto.Duration)
                .GreaterThan(0).WithMessage("Exam duration must be greater than 0 minutes.");

            RuleForEach(x => x.CreateExamDto.Questions).ChildRules(q =>
            {
                q.RuleFor(x => x.QuestionNumber)
                    .GreaterThan(0).WithMessage("Question number must be greater than 0.");

                q.RuleFor(x => x.Part)
                    .Must(part => part >= 1 && part <= 7)
                    .WithMessage("Question part must be between 1 and 7.");

                q.RuleFor(x => x.CorrectOption)
                    .NotEmpty().WithMessage("Correct answer option cannot be empty.")
                    .Must(opt => opt == "A" || opt == "B" || opt == "C" || opt == "D")
                    .WithMessage("Correct answer option must be one of: A, B, C, D.");

                q.RuleFor(x => x.OptionA)
                    .NotEmpty().WithMessage("Option A cannot be empty.");
                
                q.RuleFor(x => x.OptionB)
                    .NotEmpty().WithMessage("Option B cannot be empty.");
                
                q.RuleFor(x => x.OptionC)
                    .NotEmpty().WithMessage("Option C cannot be empty.");
                
                q.RuleFor(x => x.OptionD)
                    .NotEmpty().WithMessage("Option D cannot be empty.");

                q.When(x => x.Part == 6 || x.Part == 7, () =>
                {
                    q.RuleFor(x => x.PassageContent)
                        .NotEmpty().WithMessage("Questions in Part 6 or Part 7 require reading passage content.");
                });
            });
        });
    }
}
