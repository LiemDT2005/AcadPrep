using System.Text.Json;
using AcadPrep.Application.Common.Models;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Practice.Queries.GetPracticeSession;

public class GetPracticeSessionQueryHandler : IRequestHandler<GetPracticeSessionQuery, Result<PracticeSessionDto>>
{
    private readonly IAppDbContext _context;

    public GetPracticeSessionQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PracticeSessionDto>> Handle(GetPracticeSessionQuery request, CancellationToken cancellationToken)
    {
        var session = await _context.PracticeSessions
            .AsNoTracking()
            .Include(s => s.Exam)
            .FirstOrDefaultAsync(s => s.Id == request.SessionId && s.UserId == request.UserId, cancellationToken);

        if (session is null)
        {
            return Result<PracticeSessionDto>.Failure("Practice session not found.");
        }

        List<int> questionIds;
        try
        {
            questionIds = JsonSerializer.Deserialize<List<int>>(session.CombinedQuestionsList) ?? new List<int>();
        }
        catch
        {
            return Result<PracticeSessionDto>.Failure("Invalid practice session data.");
        }

        if (!questionIds.Any())
        {
            return Result<PracticeSessionDto>.Failure("This practice session has no questions.");
        }

        var questionsRaw = await _context.Questions
            .AsNoTracking()
            .Where(q => questionIds.Contains(q.Id))
            .Select(q => new PracticeQuestionDto
            {
                Id = q.Id,
                QuestionNumber = q.QuestionNumber,
                Part = q.Part,
                QuestionText = q.QuestionText,
                AudioUrl = q.AudioUrl,
                AudioStartSecond = q.AudioStartSecond,
                AudioEndSecond = q.AudioEndSecond,
                ImageUrl = q.ImageUrl,
                Options = q.QuestionOptions
                    .OrderBy(o => o.OptionLetter)
                    .Select(o => new PracticeQuestionOptionDto
                    {
                        Letter = o.OptionLetter.ToString(),
                        Text = o.OptionText
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        var orderMap = questionIds.Select((id, idx) => (id, idx)).ToDictionary(x => x.id, x => x.idx);
        var questions = questionsRaw.OrderBy(q => orderMap.GetValueOrDefault(q.Id, int.MaxValue)).ToList();

        var selectedParts = string.IsNullOrWhiteSpace(session.SelectedParts)
            ? new List<int>()
            : session.SelectedParts.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();

        return Result<PracticeSessionDto>.Success(new PracticeSessionDto
        {
            SessionId = session.Id,
            ExamId = session.ExamId,
            ExamTitle = session.Exam.Title,
            ExamAudioUrl = session.Exam.AudioUrl,
            TimeLimitMinutes = session.TimeLimit,
            SelectedParts = selectedParts,
            Questions = questions
        });
    }
}
