using AcadPrep.Application.Common.Models;
using Application.Common.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AcadPrep.Application.Features.Practice.Commands.SubmitPractice;

public class SubmitPracticeCommandHandler : IRequestHandler<SubmitPracticeCommand, Result<SubmitPracticeResultDto>>
{
    private readonly IAppDbContext _context;

    public SubmitPracticeCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<SubmitPracticeResultDto>> Handle(SubmitPracticeCommand request, CancellationToken cancellationToken)
    {
        var session = await _context.PracticeSessions
            .FirstOrDefaultAsync(s => s.Id == request.SessionId && s.UserId == request.UserId, cancellationToken);

        if (session is null)
        {
            return Result<SubmitPracticeResultDto>.Failure("Không tìm thấy phiên luyện tập.");
        }

        if (session.IsSubmitted)
        {
            return Result<SubmitPracticeResultDto>.Success(ToDto(session));
        }

        List<int> questionIds;
        try
        {
            questionIds = JsonSerializer.Deserialize<List<int>>(session.CombinedQuestionsList) ?? new List<int>();
        }
        catch (JsonException)
        {
            return Result<SubmitPracticeResultDto>.Failure("Dữ liệu phiên luyện tập không hợp lệ.");
        }

        if (questionIds.Count == 0)
        {
            return Result<SubmitPracticeResultDto>.Failure("Phiên luyện tập không có câu hỏi.");
        }

        var questions = await _context.Questions
            .AsNoTracking()
            .Where(q => questionIds.Contains(q.Id) && q.ExamId == session.ExamId)
            .Select(q => new { q.Id, q.Part, q.CorrectOption })
            .ToListAsync(cancellationToken);

        var questionById = questions.ToDictionary(q => q.Id);
        var answerMap = new Dictionary<int, string>();

        foreach (var (questionId, rawOption) in request.Answers)
        {
            if (!questionById.ContainsKey(questionId))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(rawOption))
            {
                continue;
            }

            answerMap[questionId] = rawOption.Trim().ToUpperInvariant();
        }

        var listeningCorrect = 0;
        var readingCorrect = 0;
        var listeningTotal = 0;
        var readingTotal = 0;
        var correctCount = 0;

        foreach (var q in questions)
        {
            if (q.Part <= 4)
            {
                listeningTotal++;
            }
            else
            {
                readingTotal++;
            }

            if (!answerMap.TryGetValue(q.Id, out var selected)
                || !Enum.TryParse<OptionLetter>(selected, out var selectedOption))
            {
                continue;
            }

            if (selectedOption != q.CorrectOption)
            {
                continue;
            }

            correctCount++;
            if (q.Part <= 4)
            {
                listeningCorrect++;
            }
            else
            {
                readingCorrect++;
            }
        }

        session.AnswersJson = JsonSerializer.Serialize(answerMap);
        session.CorrectCount = correctCount;
        session.TotalQuestions = questions.Count;
        session.ListeningCorrect = listeningCorrect;
        session.ReadingCorrect = readingCorrect;
        session.ListeningTotal = listeningTotal;
        session.ReadingTotal = readingTotal;
        session.IsSubmitted = true;
        session.CompletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<SubmitPracticeResultDto>.Success(ToDto(session));
    }

    private static SubmitPracticeResultDto ToDto(Domain.Entities.PracticeSession session) => new()
    {
        SessionId = session.Id,
        ExamId = session.ExamId,
        CorrectCount = session.CorrectCount,
        TotalQuestions = session.TotalQuestions,
        ListeningCorrect = session.ListeningCorrect,
        ReadingCorrect = session.ReadingCorrect,
        ListeningTotal = session.ListeningTotal,
        ReadingTotal = session.ReadingTotal
    };
}
