using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using AcadPrep.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.UpdateExam;

internal sealed class UpdateExamCommandHandler(IAppDbContext context)
    : IRequestHandler<UpdateExamCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(UpdateExamCommand request, CancellationToken cancellationToken)
    {
        var dto = request.UpdateExamDto;
        
        var exam = await context.Exams.FirstOrDefaultAsync(
            x => x.Id == dto.Id && !x.IsDeleted, 
            cancellationToken);

        if (exam == null)
        {
            return Result<Unit>.Failure("Exam not found or has been hidden.");
        }

        // Validate title uniqueness within same series (excluding current exam)
        var normalizedTitle = dto.Title.Trim().ToLower();
        var titleExists = await context.Exams.AnyAsync(
            x => !x.IsDeleted &&
                 x.Id != dto.Id &&
                 x.ExamSeriesId == dto.ExamSeriesId &&
                 x.Title.Trim().ToLower() == normalizedTitle,
            cancellationToken);

        if (titleExists)
        {
            return Result<Unit>.Failure("An exam with this title already exists in this series.");
        }
            
        var examSeriesExists = await context.ExamSeries.AnyAsync(
            x => x.Id == dto.ExamSeriesId && !x.IsDeleted, 
            cancellationToken);
        if (!examSeriesExists)
        {   
            return Result<Unit>.Failure("The exam series does not exist.");
        }

        exam.Title = dto.Title.Trim();
        exam.Description = dto.Description;
        exam.Duration = dto.Duration;
        exam.ExamSeriesId = dto.ExamSeriesId;
        exam.LastModifiedAt = DateTime.UtcNow;

        if (dto.ClearAudio)
        {
            exam.AudioUrl = null;
        }
        else if (!string.IsNullOrWhiteSpace(dto.AudioUrl))
        {
            exam.AudioUrl = dto.AudioUrl;
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
