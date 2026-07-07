using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using AcadPrep.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Exams.Commands.CreateExam;

internal sealed class CreateExamCommandHandler(IAppDbContext context)
    : IRequestHandler<CreateExamCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateExamCommand request, CancellationToken cancellationToken)
    {
        var dto = request.CreateExamDto;
        
        //Validate with Database
        var normalizedTitle = dto.Title.Trim().ToLower();

        var titleExists = await context.Exams.AnyAsync(
            x => !x.IsDeleted &&
                 x.ExamSeriesId == dto.ExamSeriesId &&
                 x.Title.Trim().ToLower() == normalizedTitle,
            cancellationToken);

        if (titleExists)
        {
            return Result<int>.Failure("An exam with this title already exists.");
        }
            
        var examSeriesExists = await context.ExamSeries.AnyAsync(
            x => x.Id == dto.ExamSeriesId && 
                 !x.IsDeleted, cancellationToken);
        if (!examSeriesExists)
        {   
            return Result<int>.Failure("The exam series does not exist.");
        }

        var exam = new Domain.Entities.Exam
        {
            Title = dto.Title.Trim(),
            Description = dto.Description,
            Duration = dto.Duration,
            ExamSeriesId = dto.ExamSeriesId,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        context.Exams.Add(exam);

        var success = await context.SaveChangesAsync(cancellationToken) > 0;

        if (!success)
        {
            return Result<int>.Failure("Could not save the new exam to the database.");
        }

        return Result<int>.Success(exam.Id);
    }
}
