using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcadPrep.Application.Common.Models;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Performance.Queries.GetIncorrectAnswers;

public class GetIncorrectAnswersQueryHandler : IRequestHandler<GetIncorrectAnswersQuery, Result<IncorrectAnswersResultDto>>
{
    private readonly IAppDbContext _context;

    public GetIncorrectAnswersQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IncorrectAnswersResultDto>> Handle(GetIncorrectAnswersQuery request, CancellationToken cancellationToken)
    {
        var incorrect = await _context.AttemptAnswers
            .AsNoTracking()
            .Where(a => a.ExamAttempt.UserId == request.UserId
                        && a.ExamAttempt.IsSubmitted
                        && !a.IsCorrect)
            .OrderByDescending(a => a.ExamAttempt.CompletedAt ?? a.ExamAttempt.StartedAt)
            .Select(a => new
            {
                a.QuestionId,
                a.Question.QuestionNumber,
                a.Question.Part,
                a.Question.QuestionText,
                a.Question.TopicTag,
                QuestionExplanation = a.Question.Explanation,
                GroupExplanation = a.Question.QuestionGroup != null ? a.Question.QuestionGroup.Explanation : null,
                PassageExplanation = a.Question.Passage != null ? a.Question.Passage.Explanation : null,
                CorrectOption = a.Question.CorrectOption.ToString(),
                SelectedOption = a.SelectedOption.HasValue ? a.SelectedOption.ToString() : null,
                AttemptId = a.AttemptId,
                ExamId = a.ExamAttempt.ExamId,
                ExamTitle = a.ExamAttempt.Exam.Title,
                AttemptedAt = a.ExamAttempt.CompletedAt ?? a.ExamAttempt.StartedAt,
                Options = a.Question.QuestionOptions
                    .OrderBy(o => o.OptionLetter)
                    .Select(o => new IncorrectOptionDto
                    {
                        Letter = o.OptionLetter.ToString(),
                        Text = o.OptionText
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        // Keep the latest wrong attempt per question for review focus.
        var distinctLatest = incorrect
            .GroupBy(x => x.QuestionId)
            .Select(g => g.First())
            .ToList();

        var groups = distinctLatest
            .GroupBy(x => x.Part)
            .OrderBy(g => g.Key)
            .Select(g => new IncorrectAnswerGroupDto
            {
                GroupKey = $"part-{g.Key}",
                GroupLabel = $"Part {g.Key}",
                Items = g.Select(x => new IncorrectAnswerItemDto
                {
                    QuestionId = x.QuestionId,
                    QuestionNumber = x.QuestionNumber,
                    Part = x.Part,
                    ExamId = x.ExamId,
                    ExamTitle = x.ExamTitle,
                    AttemptId = x.AttemptId,
                    QuestionText = x.QuestionText,
                    TopicTag = x.TopicTag,
                    SelectedOption = x.SelectedOption,
                    CorrectOption = x.CorrectOption,
                    Explanation = !string.IsNullOrWhiteSpace(x.QuestionExplanation)
                        ? x.QuestionExplanation
                        : !string.IsNullOrWhiteSpace(x.GroupExplanation)
                            ? x.GroupExplanation
                            : x.PassageExplanation,
                    AttemptedAt = x.AttemptedAt,
                    Options = x.Options
                }).ToList()
            })
            .ToList();

        return new IncorrectAnswersResultDto
        {
            TotalIncorrect = distinctLatest.Count,
            Groups = groups
        };
    }
}
