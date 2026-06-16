using FluentValidation;

namespace Application.Features.Exams.Queries.GetAdminExamList;

public class GetAdminExamListQueryValidator : AbstractValidator<GetAdminExamListQuery>
{
    public GetAdminExamListQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("PageNumber must be greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("PageSize must be greater than or equal to 1.");
    }
}
