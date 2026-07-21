using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcadPrep.Application.Common.Models;
using Application.Common.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.UpdatePart5Question;

internal sealed class UpdatePart5QuestionCommandHandler(IAppDbContext context)
    : IRequestHandler<UpdatePart5QuestionCommand, Result<int>>
{
    public async Task<Result<int>> Handle(UpdatePart5QuestionCommand request, CancellationToken cancellationToken)
    {
        var examExists = await context.Exams.AnyAsync(x => x.Id == request.ExamId && !x.IsDeleted, cancellationToken);
        if (!examExists)
        {
            return Result<int>.Failure("Exam not found or has been deleted.");
        }

        var hasAttempts = await context.ExamAttempts.AnyAsync(a => a.ExamId == request.ExamId, cancellationToken);
        if (hasAttempts)
        {
            return Result<int>.Failure("Cannot modify content because this exam has existing student attempts.");
        }

        var question = await context.Questions
            .Include(q => q.QuestionOptions)
            .FirstOrDefaultAsync(
                q => q.Id == request.QuestionId && q.ExamId == request.ExamId && q.Part == 5,
                cancellationToken);

        if (question == null)
        {
            return Result<int>.Failure("Question not found.");
        }

        var dto = request.Question;

        var questionNumberTaken = await context.Questions.AnyAsync(
            q => q.ExamId == request.ExamId
                 && q.QuestionNumber == dto.QuestionNumber
                 && q.Id != request.QuestionId,
            cancellationToken);

        if (questionNumberTaken)
        {
            return Result<int>.Failure($"Question number {dto.QuestionNumber} already exists in this exam.");
        }

        if (!Enum.TryParse<OptionLetter>(dto.CorrectOption, true, out var correctOptionEnum))
        {
            return Result<int>.Failure($"Invalid correct option: {dto.CorrectOption}. Must be A, B, C, or D.");
        }

        if (dto.Options.Count != 4)
        {
            return Result<int>.Failure("A question must have exactly 4 options.");
        }

        var letters = dto.Options.Select(o => o.Letter.ToUpper()).ToList();
        if (letters.Distinct().Count() != 4 || !letters.All(l => l is "A" or "B" or "C" or "D"))
        {
            return Result<int>.Failure("Options must have unique letters A, B, C, and D.");
        }

        question.QuestionNumber = dto.QuestionNumber;
        question.QuestionText = dto.QuestionText?.Trim();
        question.CorrectOption = correctOptionEnum;
        question.QuestionType = dto.QuestionType?.Trim();
        question.TopicTag = dto.TopicTag?.Trim();
        question.Explanation = string.IsNullOrWhiteSpace(dto.Explanation) ? null : dto.Explanation.Trim();

        context.QuestionOptions.RemoveRange(question.QuestionOptions);
        question.QuestionOptions.Clear();

        foreach (var optDto in dto.Options)
        {
            Enum.TryParse<OptionLetter>(optDto.Letter, true, out var optLetterEnum);
            question.QuestionOptions.Add(new Domain.Entities.QuestionOption
            {
                OptionLetter = optLetterEnum,
                OptionText = optDto.Text.Trim()
            });
        }

        var success = await context.SaveChangesAsync(cancellationToken) > 0;
        if (!success)
        {
            return Result<int>.Failure("Could not update the question.");
        }

        return Result<int>.Success(question.Id);
    }
}
