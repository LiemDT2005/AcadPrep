using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcadPrep.Application.Common.Models;
using Application.Common.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.UpdateTextCompletionSet;

internal sealed class UpdateTextCompletionSetCommandHandler(IAppDbContext context)
    : IRequestHandler<UpdateTextCompletionSetCommand, Result<int>>
{
    public async Task<Result<int>> Handle(UpdateTextCompletionSetCommand request, CancellationToken cancellationToken)
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

        var passage = await context.Passages
            .FirstOrDefaultAsync(p => p.Id == request.PassageId && p.ExamId == request.ExamId, cancellationToken);

        if (passage == null)
        {
            return Result<int>.Failure("Passage not found.");
        }

        var questions = await context.Questions
            .Include(q => q.QuestionOptions)
            .Where(q => q.ExamId == request.ExamId && q.Part == 6 && q.PassageId == request.PassageId)
            .OrderBy(q => q.QuestionNumber)
            .ToListAsync(cancellationToken);

        var dto = request.Set;
        if (questions.Count != 4 || dto.Questions.Count != 4)
        {
            return Result<int>.Failure("A Part 6 text completion set must have exactly 4 questions.");
        }

        var hasContent = !string.IsNullOrWhiteSpace(dto.Passage.Content);
        var hasImage = !string.IsNullOrWhiteSpace(dto.Passage.ImageUrl);

        if (hasContent && hasImage)
        {
            return Result<int>.Failure("A passage must use either text or an image, not both.");
        }

        if (!hasContent && !hasImage)
        {
            return Result<int>.Failure("A passage must have either text or an image.");
        }

        var existingIds = questions.Select(q => q.Id).ToHashSet();
        if (dto.Questions.Any(q => !existingIds.Contains(q.Id)))
        {
            return Result<int>.Failure("One or more questions do not belong to this passage.");
        }

        var inputQuestionNumbers = dto.Questions.Select(q => q.QuestionNumber).ToList();
        if (inputQuestionNumbers.Distinct().Count() != dto.Questions.Count)
        {
            return Result<int>.Failure("Duplicate question numbers in payload.");
        }

        var otherTaken = await context.Questions
            .Where(q => q.ExamId == request.ExamId && !existingIds.Contains(q.Id))
            .Select(q => q.QuestionNumber)
            .ToListAsync(cancellationToken);

        var conflicts = inputQuestionNumbers.Intersect(otherTaken).ToList();
        if (conflicts.Any())
        {
            return Result<int>.Failure($"Question numbers: {string.Join(", ", conflicts)} already exist in this exam.");
        }

        foreach (var q in dto.Questions)
        {
            if (!Enum.TryParse<OptionLetter>(q.CorrectOption, true, out _))
            {
                return Result<int>.Failure($"Invalid correct option: {q.CorrectOption} for question number {q.QuestionNumber}.");
            }
            if (q.Options.Count != 4)
            {
                return Result<int>.Failure($"Question {q.QuestionNumber} must have exactly 4 options.");
            }
            var letters = q.Options.Select(o => o.Letter.ToUpper()).ToList();
            if (letters.Distinct().Count() != 4 || !letters.All(l => l is "A" or "B" or "C" or "D"))
            {
                return Result<int>.Failure($"Question {q.QuestionNumber} options must have unique letters A, B, C, and D.");
            }
        }

        passage.Content = dto.Passage.Content?.Trim();
        passage.ImageUrl = dto.Passage.ImageUrl?.Trim();

        foreach (var qDto in dto.Questions)
        {
            var question = questions.First(q => q.Id == qDto.Id);
            Enum.TryParse<OptionLetter>(qDto.CorrectOption, true, out var correctOptionEnum);

            question.QuestionNumber = qDto.QuestionNumber;
            question.QuestionText = qDto.QuestionText?.Trim();
            question.CorrectOption = correctOptionEnum;

            context.QuestionOptions.RemoveRange(question.QuestionOptions);
            question.QuestionOptions.Clear();

            foreach (var optDto in qDto.Options)
            {
                Enum.TryParse<OptionLetter>(optDto.Letter, true, out var optLetterEnum);
                question.QuestionOptions.Add(new Domain.Entities.QuestionOption
                {
                    OptionLetter = optLetterEnum,
                    OptionText = optDto.Text.Trim()
                });
            }
        }

        var success = await context.SaveChangesAsync(cancellationToken) > 0;
        if (!success)
        {
            return Result<int>.Failure("Could not update the text completion set.");
        }

        return Result<int>.Success(passage.Id);
    }
}
