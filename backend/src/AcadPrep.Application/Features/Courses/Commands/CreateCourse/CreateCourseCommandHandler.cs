using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;
using MediatR;

namespace Application.Features.Courses.Commands.CreateCourse;
/*
 Sample handler
 */
internal sealed class CreateCourseCommandHandler(
    IAppDbContext context,
    ICurrentUserService currentUserService,
    TimeProvider clock) : IRequestHandler<CreateCourseCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
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
            return Result<int>.Failure("Không thể tạo mới khóa học");
        }

        return Result<int>.Success(course.Id);
    }
}
