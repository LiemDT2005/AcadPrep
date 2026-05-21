using FluentValidation;

namespace Application.Features.Courses.Commands.CreateCourse;

public class CreateCourseCommandValidator : AbstractValidator<CreateCourseCommand>
{
    public CreateCourseCommandValidator()
    {
        RuleFor(x => x.CreateCourseDto)
            .NotNull().WithMessage("Thông tin khóa học là bắt buộc");

        When(x => x.CreateCourseDto != null, () =>
        {
            RuleFor(x => x.CreateCourseDto!.Title)
                .NotEmpty().WithMessage("Tiêu đề khóa học không được để trống")
                .MaximumLength(200).WithMessage("Tiêu đề khóa học không được vượt quá 200 ký tự");

            RuleFor(x => x.CreateCourseDto!.Description)
                .NotEmpty().WithMessage("Mô tả khóa học không được để trống")
                .MaximumLength(2000).WithMessage("Mô tả khóa học không được vượt quá 2000 ký tự");

            RuleFor(x => x.CreateCourseDto!.Level)
                .NotEmpty().WithMessage("Trình độ khóa học (IELTS, TOEIC, B1, B2...) không được để trống")
                .MaximumLength(50).WithMessage("Trình độ không được vượt quá 50 ký tự");

            RuleFor(x => x.CreateCourseDto!.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Giá khóa học không được nhỏ hơn 0");
        });
    }
}
