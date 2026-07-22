using FluentValidation;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.CreateListeningGroup;

public class CreateListeningGroupCommandValidator : AbstractValidator<CreateListeningGroupCommand>
{
    public CreateListeningGroupCommandValidator()
    {
        RuleFor(x => x.ExamId)
            .GreaterThan(0).WithMessage("Exam ID must be greater than 0.");

        RuleFor(x => x.Group)
            .NotNull().WithMessage("Group details are required.");

        When(x => x.Group != null, () =>
        {
            RuleFor(x => x.Group.Part)
                .InclusiveBetween(3, 4).WithMessage("Listening group part must be 3 or 4.");

            RuleFor(x => x.Group.Name)
                .NotEmpty().WithMessage("Group name is required.");

            RuleFor(x => x.Group.Explanation)
                .NotEmpty().WithMessage("Group explanation is required.");

            RuleFor(x => x.Group.Questions)
                .NotNull().WithMessage("Questions are required.")
                .Must(q => q.Count == 3).WithMessage("Listening group must have exactly 3 questions.");
        });
    }
}
