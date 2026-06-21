using FluentValidation;

namespace Application.Features.Exam.Queries.GetExamDetail;

public class GetExamDetailQueryValidator : AbstractValidator<GetExamDetailQuery>
{
    public GetExamDetailQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Exam Id is not valid");
    }
}