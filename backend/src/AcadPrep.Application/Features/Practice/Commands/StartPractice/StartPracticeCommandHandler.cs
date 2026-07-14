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

        var questionIds = await query.Select(q => q.Id).ToListAsync(cancellationToken);

        if (!questionIds.Any())
        {
            return Result<int>.Failure("No questions match your selected parts or tags. Please adjust your selection.");
        }

        // Trộn ngẫu nhiên câu hỏi để tăng hiệu quả luyện tập
        var rnd = new Random();
        var shuffledIds = questionIds.OrderBy(x => rnd.Next()).ToList();
        var jsonQuestionsList = JsonSerializer.Serialize(shuffledIds);

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
