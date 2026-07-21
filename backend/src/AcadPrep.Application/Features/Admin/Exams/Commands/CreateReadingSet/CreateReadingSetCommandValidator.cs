using FluentValidation;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.CreateReadingSet;

public class CreateReadingSetCommandValidator : AbstractValidator<CreateReadingSetCommand>
{
    public CreateReadingSetCommandValidator()
    {
        RuleFor(x => x.ExamId)
            .GreaterThan(0).WithMessage("Exam ID must be greater than 0.");

        RuleFor(x => x.Set)
            .NotNull().WithMessage("Set details are required.");

        When(x => x.Set != null, () =>
        {
            RuleFor(x => x.Set.Name)
                .NotEmpty().WithMessage("Reading set name is required.");

            RuleFor(x => x.Set.Explanation)
                .NotEmpty().WithMessage("Set explanation is required.");
        });
    }
}
