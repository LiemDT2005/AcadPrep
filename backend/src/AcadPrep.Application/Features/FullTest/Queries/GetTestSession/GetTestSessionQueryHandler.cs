using AcadPrep.Application.Common.Models;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.FullTest.Queries.GetTestSession;

public class GetTestSessionQueryHandler : IRequestHandler<GetTestSessionQuery, Result<TestSessionDto>>
{
    private readonly IAppDbContext _context;

    public GetTestSessionQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<TestSessionDto>> Handle(GetTestSessionQuery request, CancellationToken cancellationToken)
    {
        var attempt = await _context.ExamAttempts
            .AsNoTracking()
            .Include(a => a.Exam)
            .Include(a => a.AttemptAnswers)
            .FirstOrDefaultAsync(a => a.Id == request.AttemptId && a.UserId == request.UserId, cancellationToken);

        if (attempt is null)
        {
            return Result<TestSessionDto>.Failure("Test session not found.");
        }

        if (attempt.IsSubmitted)
        {
            return Result<TestSessionDto>.Failure("This test has already been submitted.");
        }

        var questions = await _context.Questions
            .AsNoTracking()
            .Where(q => q.ExamId == attempt.ExamId)
            .OrderBy(q => q.QuestionNumber)
            .Select(q => new TestQuestionDto
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
                    .Select(o => new TestQuestionOptionDto
                    {
                        Letter = o.OptionLetter.ToString(),
                        Text = o.OptionText
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        var savedAnswers = attempt.AttemptAnswers
            .Where(a => a.SelectedOption.HasValue)
            .ToDictionary(a => a.QuestionId, a => a.SelectedOption!.Value.ToString());

        var answeredCount = savedAnswers.Count;
        var currentIndex = answeredCount > 0 && answeredCount < questions.Count
            ? answeredCount
            : Math.Min(answeredCount, questions.Count - 1);

        return Result<TestSessionDto>.Success(new TestSessionDto
        {
            AttemptId = attempt.Id,
            ExamId = attempt.ExamId,
            ExamTitle = attempt.Exam.Title,
            ExamAudioUrl = attempt.Exam.AudioUrl,
            RemainingSeconds = attempt.RemainingTime,
            IsSubmitted = attempt.IsSubmitted,
            CurrentQuestionIndex = currentIndex,
            Questions = questions,
            SavedAnswers = savedAnswers
        });
    }
}
