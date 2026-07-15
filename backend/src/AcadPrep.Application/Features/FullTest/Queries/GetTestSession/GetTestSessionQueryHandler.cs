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

        var questionsRaw = await _context.Questions
            .AsNoTracking()
            .Where(q => q.ExamId == attempt.ExamId)
            .OrderBy(q => q.QuestionNumber)
            .Select(q => new
            {
                q.Id,
                q.QuestionNumber,
                q.Part,
                q.QuestionText,
                q.AudioUrl,
                q.AudioStartSecond,
                q.AudioEndSecond,
                q.ImageUrl,
                q.PassageId,
                PassageContent = q.Passage != null ? q.Passage.Content : null,
                PassageImageUrl = q.Passage != null ? q.Passage.ImageUrl : null,
                PassageDisplayOrder = q.Passage != null ? q.Passage.DisplayOrder : (int?)null,
                QuestionGroupId = q.QuestionGroupId,
                GroupAudioUrl = q.QuestionGroup != null ? q.QuestionGroup.AudioUrl : null,
                GroupAudioStartSecond = q.QuestionGroup != null ? q.QuestionGroup.AudioStartSecond : null,
                GroupAudioEndSecond = q.QuestionGroup != null ? q.QuestionGroup.AudioEndSecond : null,
                GroupImageUrl = q.QuestionGroup != null ? q.QuestionGroup.ImageUrl : null,
                GroupPassages = q.QuestionGroup != null
                    ? q.QuestionGroup.Passages
                        .OrderBy(p => p.DisplayOrder)
                        .Select(p => new SessionPassageDto
                        {
                            DisplayOrder = p.DisplayOrder,
                            Content = p.Content,
                            ImageUrl = p.ImageUrl
                        })
                        .ToList()
                    : new List<SessionPassageDto>(),
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

        var questions = questionsRaw.Select(q => new TestQuestionDto
        {
            Id = q.Id,
            QuestionNumber = q.QuestionNumber,
            Part = q.Part,
            QuestionText = q.QuestionText,
            AudioUrl = q.AudioUrl,
            AudioStartSecond = q.AudioStartSecond,
            AudioEndSecond = q.AudioEndSecond,
            ImageUrl = q.ImageUrl,
            PassageId = q.PassageId,
            QuestionGroupId = q.QuestionGroupId,
            GroupAudioUrl = q.GroupAudioUrl,
            GroupAudioStartSecond = q.GroupAudioStartSecond,
            GroupAudioEndSecond = q.GroupAudioEndSecond,
            GroupImageUrl = q.GroupImageUrl,
            Passages = BuildPassages(q.PassageId, q.PassageContent, q.PassageImageUrl, q.PassageDisplayOrder, q.GroupPassages),
            Options = q.Options
        }).ToList();

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

    private static List<SessionPassageDto> BuildPassages(
        int? passageId,
        string? passageContent,
        string? passageImageUrl,
        int? passageDisplayOrder,
        List<SessionPassageDto> groupPassages)
    {
        if (groupPassages.Count > 0)
        {
            return groupPassages;
        }

        if (passageId.HasValue && (passageContent != null || passageImageUrl != null))
        {
            return
            [
                new SessionPassageDto
                {
                    DisplayOrder = passageDisplayOrder ?? 1,
                    Content = passageContent,
                    ImageUrl = passageImageUrl
                }
            ];
        }

        return [];
    }
}
