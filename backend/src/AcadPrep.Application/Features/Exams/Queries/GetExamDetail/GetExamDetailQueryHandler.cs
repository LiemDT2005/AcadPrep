using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using AcadPrep.Application.Common.Models;
using Application.Features.Exams.Queries.Common.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Exams.Queries.GetExamDetail;

internal sealed class GetExamDetailQueryHandler(IAppDbContext context)
    : IRequestHandler<GetExamDetailQuery, Result<ExamDetailDto>>
{
    public async Task<Result<ExamDetailDto>> Handle(
        GetExamDetailQuery request, CancellationToken cancellationToken)
    {
        var exam = await context.Exams
            .IgnoreQueryFilters()
            .Include(x => x.Questions)
                .ThenInclude(q => q.QuestionOptions)
            .Include(x => x.Questions)
                .ThenInclude(q => q.Passage)
            .Include(x => x.QuestionGroups)
                .ThenInclude(g => g.Passages)
            .Include(x => x.QuestionGroups)
                .ThenInclude(g => g.Questions)
                    .ThenInclude(q => q.QuestionOptions)
            .Include(x => x.ExamAttempts)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (exam == null)
        {
            return Result<ExamDetailDto>.Failure("Exam not found.");
        }

        var dto = new ExamDetailDto
        {
            Id = exam.Id,
            Title = exam.Title,
            Description = exam.Description,
            Duration = exam.Duration,
            IsDeleted = exam.IsDeleted,
            CreatedAt = exam.CreatedAt,
            AttemptCount = exam.ExamAttempts.Count,
            Questions = exam.Questions
                .OrderBy(q => q.QuestionNumber)
                .Select(MapQuestion)
                .ToList(),
            Part7ReadingSets = exam.QuestionGroups
                .Where(g => g.Questions.Any(q => q.Part == 7))
                .OrderBy(g => g.Questions.Where(q => q.Part == 7).Min(q => q.QuestionNumber))
                .Select(g => new ReadingSetDto
                {
                    QuestionGroupId = g.Id,
                    Name = g.Name,
                    Passages = g.Passages
                        .OrderBy(p => p.DisplayOrder)
                        .Select(p => new PassageDetailDto
                        {
                            Id = p.Id,
                            DisplayOrder = p.DisplayOrder,
                            Content = p.Content,
                            ImageUrl = p.ImageUrl
                        })
                        .ToList(),
                    Questions = g.Questions
                        .Where(q => q.Part == 7)
                        .OrderBy(q => q.QuestionNumber)
                        .Select(MapQuestion)
                        .ToList()
                })
                .ToList()
        };

        return Result<ExamDetailDto>.Success(dto);
    }

    private static QuestionDetailDto MapQuestion(Domain.Entities.Question q) => new()
    {
        Id = q.Id,
        QuestionNumber = q.QuestionNumber,
        Part = q.Part,
        QuestionText = q.QuestionText,
        AudioUrl = q.AudioUrl,
        CorrectOption = q.CorrectOption.ToString(),
        PassageId = q.PassageId,
        PassageContent = q.Passage?.Content,
        PassageImageUrl = q.Passage?.ImageUrl,
        QuestionGroupId = q.QuestionGroupId,
        Options = q.QuestionOptions
            .OrderBy(o => o.OptionLetter)
            .Select(o => new QuestionOptionDto
            {
                OptionLetter = o.OptionLetter.ToString(),
                OptionText = o.OptionText
            })
            .ToList()
    };
}
