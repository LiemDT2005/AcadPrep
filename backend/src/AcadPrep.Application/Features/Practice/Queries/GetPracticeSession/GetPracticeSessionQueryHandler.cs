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

        if (session.IsSubmitted)
        {
            return Result<PracticeSessionDto>.Failure("This practice session has already been submitted.");
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
                    .Select(o => new PracticeQuestionOptionDto
                    {
                        Letter = o.OptionLetter.ToString(),
                        Text = o.OptionText
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        var orderMap = questionIds.Select((id, idx) => (id, idx)).ToDictionary(x => x.id, x => x.idx);
        var questions = questionsRaw
            .OrderBy(q => orderMap.GetValueOrDefault(q.Id, int.MaxValue))
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
                PassageId = q.PassageId,
                QuestionGroupId = q.QuestionGroupId,
                GroupAudioUrl = q.GroupAudioUrl,
                GroupAudioStartSecond = q.GroupAudioStartSecond,
                GroupAudioEndSecond = q.GroupAudioEndSecond,
                GroupImageUrl = q.GroupImageUrl,
                Passages = BuildPassages(q.PassageId, q.PassageContent, q.PassageImageUrl, q.PassageDisplayOrder, q.GroupPassages),
                Options = q.Options
            })
            .ToList();

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
