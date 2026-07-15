using AcadPrep.Application.Common.Models;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AcadPrep.Application.Features.Practice.Commands.StartPractice;

public class StartPracticeCommandHandler : IRequestHandler<StartPracticeCommand, Result<int>>
{
    private readonly IAppDbContext _context;

    public StartPracticeCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(StartPracticeCommand request, CancellationToken cancellationToken)
    {
        // 1. Kiểm tra đề thi tồn tại
        var examExists = await _context.Exams.AnyAsync(e => e.Id == request.ExamId && !e.IsDeleted, cancellationToken);
        if (!examExists)
        {
            return Result<int>.Failure("Exam not found or has been deleted.");
        }

        // 2. Query danh sách câu hỏi thuộc đề thi
        var query = _context.Questions
            .AsNoTracking()
            .Where(q => q.ExamId == request.ExamId);

        // 3. Lọc theo Part nếu được chọn
        if (request.SelectedPartNumbers != null && request.SelectedPartNumbers.Any())
        {
            query = query.Where(q => request.SelectedPartNumbers.Contains(q.Part));
        }

        // 4. Lọc theo Tag nếu được chọn
        if (request.SelectedTags != null && request.SelectedTags.Any())
        {
            query = query.Where(q => q.TopicTag != null && request.SelectedTags.Contains(q.TopicTag));
        }

        var questions = await query
            .Select(q => new { q.Id, q.Part, q.QuestionGroupId, q.PassageId, q.QuestionNumber })
            .ToListAsync(cancellationToken);

        if (!questions.Any())
        {
            return Result<int>.Failure("No questions match your selected parts or tags. Please adjust your selection.");
        }

        // Keep Listening groups (3/4), Part 6 passages, and Part 7 reading sets together,
        // then order units by question number so Next follows the part sequence.
        var units = new List<(int SortKey, List<int> Ids)>();

        units.AddRange(questions
            .Where(q => (q.Part is 3 or 4 or 7) && q.QuestionGroupId.HasValue)
            .GroupBy(q => q.QuestionGroupId!.Value)
            .Select(g =>
            {
                var ordered = g.OrderBy(x => x.QuestionNumber).ToList();
                return (ordered[0].QuestionNumber, ordered.Select(x => x.Id).ToList());
            }));

        units.AddRange(questions
            .Where(q => q.Part == 6 && q.PassageId.HasValue)
            .GroupBy(q => q.PassageId!.Value)
            .Select(g =>
            {
                var ordered = g.OrderBy(x => x.QuestionNumber).ToList();
                return (ordered[0].QuestionNumber, ordered.Select(x => x.Id).ToList());
            }));

        units.AddRange(questions
            .Where(q => !((q.Part is 3 or 4 or 7) && q.QuestionGroupId.HasValue)
                        && !(q.Part == 6 && q.PassageId.HasValue))
            .Select(q => (q.QuestionNumber, new List<int> { q.Id })));

        var orderedIds = units
            .OrderBy(u => u.SortKey)
            .SelectMany(u => u.Ids)
            .ToList();
        var jsonQuestionsList = JsonSerializer.Serialize(orderedIds);

        // 5. Khởi tạo phiên luyện tập mới
        var session = new PracticeSession
        {
            UserId = request.UserId,
            ExamId = request.ExamId,
            SelectedParts = request.SelectedPartNumbers != null ? string.Join(",", request.SelectedPartNumbers) : string.Empty,
            SelectedTags = request.SelectedTags != null ? string.Join(",", request.SelectedTags) : string.Empty,
            TimeLimit = request.TimeLimitMinutes,
            CombinedQuestionsList = jsonQuestionsList,
            CreatedAt = DateTime.UtcNow
        };

        _context.PracticeSessions.Add(session);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(session.Id);
    }
}
