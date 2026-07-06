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
        // Truy vấn đề thi cùng các mối quan hệ (câu hỏi, đáp án lựa chọn, đoạn văn, lượt thi)
        var exam = await context.Exams
            .IgnoreQueryFilters()
            .Include(x => x.Questions)
                .ThenInclude(q => q.QuestionOptions)
            .Include(x => x.Questions)
                .ThenInclude(q => q.Passage)
            .Include(x => x.ExamAttempts)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (exam == null)
        {
            return Result<ExamDetailDto>.Failure("Exam not found.");
        }

        // Ánh xạ sang DTO
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
                .Select(q => new QuestionDetailDto
                {
                    Id = q.Id,
                    QuestionNumber = q.QuestionNumber,
                    Part = q.Part,
                    QuestionText = q.QuestionText,
                    AudioUrl = q.AudioUrl,
                    CorrectOption = q.CorrectOption.ToString(),
                    PassageId = q.PassageId,
                    PassageContent = q.Passage?.Content,
                    Options = q.QuestionOptions
                        .OrderBy(o => o.OptionLetter)
                        .Select(o => new QuestionOptionDto
                        {
                            OptionLetter = o.OptionLetter.ToString(),
                            OptionText = o.OptionText
                        })
                        .ToList()
                })
                .ToList()
        };

        return Result<ExamDetailDto>.Success(dto);
    }
}
