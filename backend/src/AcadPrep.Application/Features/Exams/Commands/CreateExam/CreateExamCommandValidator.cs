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
                .GreaterThan(0).LessThanOrEqualTo(120).WithMessage("Exam duration must be greater than 0 minutes and lower than 120 seconds.");
            
            RuleFor(x => x.CreateExamDto.Description)
                .MaximumLength(1000).WithMessage("Exam description cannot exceed 1000 characters.");

            RuleFor(x => x.CreateExamDto.ExamSeriesId)
                .GreaterThan(0).WithMessage("Exam series is required.");
        });
    }
}
