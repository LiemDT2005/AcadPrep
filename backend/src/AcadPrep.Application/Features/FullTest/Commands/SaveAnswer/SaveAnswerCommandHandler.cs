using AcadPrep.Application.Common.Models;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.FullTest.Commands.SaveAnswer;

public class SaveAnswerCommandHandler : IRequestHandler<SaveAnswerCommand, Result>
{
    private readonly IAppDbContext _context;

    public SaveAnswerCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(SaveAnswerCommand request, CancellationToken cancellationToken)
    {
        var attempt = await _context.ExamAttempts
            .FirstOrDefaultAsync(a => a.Id == request.AttemptId && a.UserId == request.UserId && !a.IsSubmitted, cancellationToken);

        if (attempt is null)
        {
            return Result.Failure("Phiên thi không hợp lệ hoặc đã nộp bài.");
        }

        var question = await _context.Questions
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == request.QuestionId && q.ExamId == attempt.ExamId, cancellationToken);

        if (question is null)
        {
            return Result.Failure("Câu hỏi không thuộc đề thi này.");
        }

        OptionLetter? selected = null;
        if (!string.IsNullOrWhiteSpace(request.SelectedOption) &&
            Enum.TryParse<OptionLetter>(request.SelectedOption.Trim().ToUpperInvariant(), out var parsed))
        {
            selected = parsed;
        }

        var existing = await _context.AttemptAnswers
            .FirstOrDefaultAsync(a => a.AttemptId == request.AttemptId && a.QuestionId == request.QuestionId, cancellationToken);

        if (existing is null)
        {
            _context.AttemptAnswers.Add(new AttemptAnswer
            {
                AttemptId = request.AttemptId,
                QuestionId = request.QuestionId,
                SelectedOption = selected,
                IsCorrect = selected.HasValue && selected.Value == question.CorrectOption
            });
        }
        else
        {
            existing.SelectedOption = selected;
            existing.IsCorrect = selected.HasValue && selected.Value == question.CorrectOption;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
