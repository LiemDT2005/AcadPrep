using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using MediatR;

namespace Application.Features.Courses.Commands.CreateCourse;

internal sealed class CreateCourseCommandHandler(
    IAppDbContext context,
    ICurrentUserService currentUserService,
    TimeProvider clock) : IRequestHandler<CreateCourseCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
    {
        // 1. Initialize entity using domain factory method
        var course = Course.Create(
            request.CreateCourseDto.Title,
            request.CreateCourseDto.Description,
            request.CreateCourseDto.Level,
            request.CreateCourseDto.Price,
            currentUserService.UserId ?? "System",
            clock.GetUtcNow()
        );

        // 2. Add and save changes
        context.Courses.Add(course);
        var success = await context.SaveChangesAsync(cancellationToken) > 0;

        if (!success)
        {
            return Result<string>.Failure("Không thể tạo mới khóa học", 400);
        }

        return Result<string>.Success("Tạo khóa học mới thành công", course.Id);
    }
}
