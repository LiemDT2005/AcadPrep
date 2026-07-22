using FluentValidation;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.UpdateListeningGroup;

public class UpdateListeningGroupCommandValidator : AbstractValidator<UpdateListeningGroupCommand>
{
    public UpdateListeningGroupCommandValidator()
    {
        RuleFor(x => x.ExamId).GreaterThan(0);
        RuleFor(x => x.QuestionGroupId).GreaterThan(0);
        RuleFor(x => x.Group).NotNull();

        When(x => x.Group != null, () =>
        {
            RuleFor(x => x.Group.Name).NotEmpty().WithMessage("Group name is required.");
            RuleFor(x => x.Group.Explanation).NotEmpty().WithMessage("Group explanation is required.");
        });
    }
}
