using FluentValidation;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.UpdateExam;

public class UpdateExamCommandValidator : AbstractValidator<UpdateExamCommand>
{
    public UpdateExamCommandValidator()
    {
        RuleFor(x => x.UpdateExamDto)
            .NotNull().WithMessage("Exam information is required.");

        When(x => x.UpdateExamDto != null, () =>
        {
            RuleFor(x => x.UpdateExamDto.Title)
                .NotEmpty().WithMessage("Exam title cannot be empty.")
                .MaximumLength(255).WithMessage("Exam title cannot exceed 255 characters.");

            RuleFor(x => x.UpdateExamDto.Duration)
                .GreaterThan(0).LessThanOrEqualTo(120).WithMessage("Exam duration must be greater than 0 minutes and less than or equal to 120 minutes.");

            RuleFor(x => x.UpdateExamDto.Description)
                .MaximumLength(1000).WithMessage("Exam description cannot exceed 1000 characters.");

            RuleFor(x => x.UpdateExamDto.ExamSeriesId)
                .GreaterThan(0).WithMessage("Exam series is required.");
        });
    }
}
