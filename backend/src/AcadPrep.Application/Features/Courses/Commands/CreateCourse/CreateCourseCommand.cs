using Application.Common.Models;
using MediatR;

namespace Application.Features.Courses.Commands.CreateCourse;

public class CreateCourseCommand : IRequest<Result<int>>
{
    public required CreateCourseDto CreateCourseDto { get; set; }
}
