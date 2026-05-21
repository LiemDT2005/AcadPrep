using Domain.Common;
using MediatR;

namespace Application.Features.Courses.Commands.CreateCourse;

public class CreateCourseCommand : IRequest<Result<string>>
{
    public required CreateCourseDto CreateCourseDto { get; set; }
}
