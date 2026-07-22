using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcadPrep.Application.Common.Models;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Performance.Queries.GetStudyHistory;

public class GetStudyHistoryQueryHandler : IRequestHandler<GetStudyHistoryQuery, Result<StudyHistoryResultDto>>
{
    private readonly IAppDbContext _context;

    public GetStudyHistoryQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<StudyHistoryResultDto>> Handle(GetStudyHistoryQuery request, CancellationToken cancellationToken)
    {
        var items = new List<StudyHistoryItemDto>();

        var examAttempts = await _context.ExamAttempts
            .AsNoTracking()
            .Include(a => a.Exam)
            .Where(a => a.UserId == request.UserId && a.IsSubmitted && a.CompletedAt != null)
            .OrderByDescending(a => a.CompletedAt)
            .Take(50)
            .ToListAsync(cancellationToken);

        foreach (var attempt in examAttempts)
        {
            items.Add(new StudyHistoryItemDto
            {
                ActivityType = "exam",
                Title = attempt.Exam.Title,
                Description = $"Completed full test — scored {attempt.TotalScore}/990",
                OccurredAt = attempt.CompletedAt!.Value,
                LinkUrl = $"/Exams/Review?attemptId={attempt.Id}",
                Icon = "quiz",
                ColorType = "primary"
            });
        }

        var practiceSessions = await _context.PracticeSessions
            .AsNoTracking()
            .Include(s => s.Exam)
            .Where(s => s.UserId == request.UserId && s.IsSubmitted && s.CompletedAt != null)
            .OrderByDescending(s => s.CompletedAt)
            .Take(50)
            .ToListAsync(cancellationToken);

        foreach (var session in practiceSessions)
        {
            items.Add(new StudyHistoryItemDto
            {
                ActivityType = "practice",
                Title = session.Exam.Title,
                Description = $"Completed practice — {session.CorrectCount}/{session.TotalQuestions} correct",
                OccurredAt = session.CompletedAt!.Value,
                LinkUrl = $"/Exams/Review?sessionId={session.Id}",
                Icon = "fitness_center",
                ColorType = "secondary"
            });
        }

        var vocabGroups = await _context.SavedVocabularies
            .AsNoTracking()
            .Where(v => v.UserId == request.UserId)
            .GroupBy(v => v.DateSaved.Date)
            .Select(g => new { Date = g.Key, Count = g.Count(), Latest = g.Max(x => x.DateSaved) })
            .OrderByDescending(g => g.Latest)
            .Take(30)
            .ToListAsync(cancellationToken);

        foreach (var group in vocabGroups)
        {
            items.Add(new StudyHistoryItemDto
            {
                ActivityType = "vocab",
                Title = "Vocabulary notebook",
                Description = $"Saved {group.Count} word{(group.Count > 1 ? "s" : "")}",
                OccurredAt = group.Latest,
                LinkUrl = "/Vocabulary",
                Icon = "menu_book",
                ColorType = "tertiary"
            });
        }

        return new StudyHistoryResultDto
        {
            Items = items.OrderByDescending(i => i.OccurredAt).ToList()
        };
    }
}
