using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AcadPrep.Application.Common.Models;
using Application.Common.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.FullTest.Queries.GetAttemptReview;

public class GetAttemptReviewQueryHandler : IRequestHandler<GetAttemptReviewQuery, Result<AttemptReviewDto>>
{
    private readonly IAppDbContext _context;

    public GetAttemptReviewQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<AttemptReviewDto>> Handle(GetAttemptReviewQuery request, CancellationToken cancellationToken)
    {
        if (request.AttemptId.HasValue)
        {
            return await LoadFullTestReviewAsync(request.AttemptId.Value, request.UserId, cancellationToken);
        }

        if (request.SessionId.HasValue)
        {
            return await LoadPracticeReviewAsync(request.SessionId.Value, request.UserId, cancellationToken);
        }

        return Result<AttemptReviewDto>.Failure("Missing attempt or session id.");
    }

    private async Task<Result<AttemptReviewDto>> LoadFullTestReviewAsync(int attemptId, int userId, CancellationToken cancellationToken)
    {
        var attempt = await _context.ExamAttempts
            .AsNoTracking()
            .Include(a => a.Exam)
            .FirstOrDefaultAsync(a => a.Id == attemptId && a.UserId == userId && a.IsSubmitted, cancellationToken);

        if (attempt is null)
        {
            return Result<AttemptReviewDto>.Failure("Không tìm thấy kết quả bài thi.");
        }

        var answers = await _context.AttemptAnswers
            .AsNoTracking()
            .Where(a => a.AttemptId == attemptId)
            .ToListAsync(cancellationToken);

        var answerByQuestionId = answers.ToDictionary(a => a.QuestionId);

        var questions = await _context.Questions
            .AsNoTracking()
            .Include(q => q.QuestionOptions)
            .Include(q => q.Passage)
            .Include(q => q.QuestionGroup)
            .Where(q => q.ExamId == attempt.ExamId)
            .OrderBy(q => q.QuestionNumber)
            .ToListAsync(cancellationToken);

        var reviewQuestions = questions.Select(q =>
        {
            answerByQuestionId.TryGetValue(q.Id, out var answer);
            var selected = answer?.SelectedOption;
            var isAnswered = selected.HasValue;
            var isCorrect = answer?.IsCorrect == true;

            return MapQuestion(q, selected?.ToString(), isAnswered, isCorrect);
        }).ToList();

        return new AttemptReviewDto
        {
            IsPractice = false,
            AttemptOrSessionId = attempt.Id,
            ExamId = attempt.ExamId,
            ExamTitle = attempt.Exam.Title,
            TotalScore = attempt.TotalScore,
            MaxScore = 990,
            ListeningScore = attempt.ListeningScore,
            ReadingScore = attempt.ReadingScore,
            CorrectCount = reviewQuestions.Count(q => q.IsCorrect),
            IncorrectCount = reviewQuestions.Count(q => q.IsAnswered && !q.IsCorrect),
            UnansweredCount = reviewQuestions.Count(q => !q.IsAnswered),
            CompletedAt = attempt.CompletedAt,
            Questions = reviewQuestions
        };
    }

    private async Task<Result<AttemptReviewDto>> LoadPracticeReviewAsync(int sessionId, int userId, CancellationToken cancellationToken)
    {
        var session = await _context.PracticeSessions
            .AsNoTracking()
            .Include(s => s.Exam)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId && s.IsSubmitted, cancellationToken);

        if (session is null)
        {
            return Result<AttemptReviewDto>.Failure("Không tìm thấy kết quả luyện tập.");
        }

        List<int> questionIds;
        try
        {
            questionIds = JsonSerializer.Deserialize<List<int>>(session.CombinedQuestionsList) ?? new List<int>();
        }
        catch (JsonException)
        {
            return Result<AttemptReviewDto>.Failure("Dữ liệu phiên luyện tập không hợp lệ.");
        }

        var answerMap = new Dictionary<int, string>();
        if (!string.IsNullOrWhiteSpace(session.AnswersJson))
        {
            try
            {
                answerMap = JsonSerializer.Deserialize<Dictionary<int, string>>(session.AnswersJson)
                            ?? new Dictionary<int, string>();
            }
            catch (JsonException)
            {
                // Treat as unanswered if answers cannot be parsed.
            }
        }

        var questions = await _context.Questions
            .AsNoTracking()
            .Include(q => q.QuestionOptions)
            .Include(q => q.Passage)
            .Include(q => q.QuestionGroup)
            .Where(q => questionIds.Contains(q.Id))
            .ToListAsync(cancellationToken);

        var ordered = questionIds
            .Select(id => questions.FirstOrDefault(q => q.Id == id))
            .Where(q => q is not null)
            .Select(q => q!)
            .ToList();

        var reviewQuestions = ordered.Select(q =>
        {
            answerMap.TryGetValue(q.Id, out var selectedRaw);
            var selected = string.IsNullOrWhiteSpace(selectedRaw) ? null : selectedRaw.Trim().ToUpperInvariant();
            var isAnswered = selected is not null && Enum.TryParse<OptionLetter>(selected, out var selectedLetter);
            var isCorrect = isAnswered
                            && Enum.TryParse<OptionLetter>(selected, out var parsed)
                            && parsed == q.CorrectOption;
            return MapQuestion(q, isAnswered ? selected : null, isAnswered, isCorrect);
        }).ToList();

        return new AttemptReviewDto
        {
            IsPractice = true,
            AttemptOrSessionId = session.Id,
            ExamId = session.ExamId,
            ExamTitle = session.Exam.Title,
            TotalScore = session.CorrectCount,
            MaxScore = session.TotalQuestions,
            ListeningScore = session.ListeningCorrect,
            ReadingScore = session.ReadingCorrect,
            CorrectCount = reviewQuestions.Count(q => q.IsCorrect),
            IncorrectCount = reviewQuestions.Count(q => q.IsAnswered && !q.IsCorrect),
            UnansweredCount = reviewQuestions.Count(q => !q.IsAnswered),
            CompletedAt = session.CompletedAt,
            Questions = reviewQuestions
        };
    }

    private static ReviewQuestionDto MapQuestion(
        Domain.Entities.Question q,
        string? selectedOption,
        bool isAnswered,
        bool isCorrect)
    {
        return new ReviewQuestionDto
        {
            QuestionId = q.Id,
            QuestionNumber = q.QuestionNumber,
            Part = q.Part,
            QuestionText = q.QuestionText,
            ImageUrl = q.ImageUrl,
            AudioUrl = q.AudioUrl,
            PassageContent = q.Passage?.Content,
            PassageImageUrl = q.Passage?.ImageUrl,
            TopicTag = q.TopicTag,
            SelectedOption = selectedOption,
            CorrectOption = q.CorrectOption.ToString(),
            IsCorrect = isCorrect,
            IsAnswered = isAnswered,
            Explanation = ResolveExplanation(q),
            Options = q.QuestionOptions
                .OrderBy(o => o.OptionLetter)
                .Select(o => new ReviewOptionDto
                {
                    Letter = o.OptionLetter.ToString(),
                    Text = o.OptionText
                })
                .ToList()
        };
    }

    private static string? ResolveExplanation(Domain.Entities.Question q)
    {
        if (!string.IsNullOrWhiteSpace(q.Explanation))
        {
            return q.Explanation;
        }

        if (!string.IsNullOrWhiteSpace(q.QuestionGroup?.Explanation))
        {
            return q.QuestionGroup.Explanation;
        }

        if (!string.IsNullOrWhiteSpace(q.Passage?.Explanation))
        {
            return q.Passage.Explanation;
        }

        return null;
    }
}
