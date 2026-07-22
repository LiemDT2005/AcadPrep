using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using AcadPrep.Application.Common.Models;
using AcadPrep.Application.Common.Utils;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.CreateTextCompletionSet;

internal sealed class CreateTextCompletionSetCommandHandler(IAppDbContext context)
    : IRequestHandler<CreateTextCompletionSetCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateTextCompletionSetCommand request, CancellationToken cancellationToken)
    {
        var exam = await context.Exams
            .FirstOrDefaultAsync(x => x.Id == request.ExamId && !x.IsDeleted, cancellationToken);

        if (exam == null)
        {
            return Result<int>.Failure("Exam not found or has been deleted.");
        }

        var dto = request.Set;

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

        var part6Count = await context.Questions
            .CountAsync(q => q.ExamId == request.ExamId && q.Part == 6, cancellationToken);

        if (!ToeicPartLimits.CanAddQuestionCount(6, part6Count, ToeicPartLimits.TextCompletionQuestionCount))
        {
            return Result<int>.Failure(
                $"Part 6 already has the maximum of {ToeicPartLimits.GetLimit(6)} questions.");
        }

        if (dto.Questions.Count != 4)
        {
            return Result<int>.Failure("A Part 6 text completion set must have exactly 4 questions.");
        }

        var inputQuestionNumbers = dto.Questions.Select(q => q.QuestionNumber).ToList();
        if (inputQuestionNumbers.Distinct().Count() != dto.Questions.Count)
        {
            return Result<int>.Failure("Duplicate question numbers in payload.");
        }

        var existingQuestionNumbers = await context.Questions
            .Where(q => q.ExamId == request.ExamId)
            .Select(q => q.QuestionNumber)
            .ToListAsync(cancellationToken);

        var takenNumbers = inputQuestionNumbers.Intersect(existingQuestionNumbers).ToList();
        if (takenNumbers.Any())
        {
            return Result<int>.Failure($"Question numbers: {string.Join(", ", takenNumbers)} already exist in this exam.");
        }

        // Validate each question correct option & option count
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
            if (letters.Distinct().Count() != 4 || !letters.All(l => l == "A" || l == "B" || l == "C" || l == "D"))
            {
                return Result<int>.Failure($"Question {q.QuestionNumber} options must have unique letters A, B, C, and D.");
            }
        }

        var nextDisplayOrder = await context.Questions
            .Where(q => q.ExamId == request.ExamId && q.Part == 6 && q.PassageId != null)
            .Select(q => q.PassageId)
            .Distinct()
            .CountAsync(cancellationToken) + 1;

        // Create Passage
        var passage = new Passage
        {
            ExamId = request.ExamId,
            Content = dto.Passage.Content?.Trim(),
            ImageUrl = dto.Passage.ImageUrl?.Trim(),
            Explanation = string.IsNullOrWhiteSpace(dto.Passage.Explanation) ? null : dto.Passage.Explanation.Trim(),
            DisplayOrder = nextDisplayOrder,
            QuestionGroupId = null // Part 6 has no QuestionGroup
        };

        context.Passages.Add(passage);

        // Add questions to passage
        foreach (var qDto in dto.Questions)
        {
            Enum.TryParse<OptionLetter>(qDto.CorrectOption, true, out var correctOptionEnum);
            var question = new Question
            {
                ExamId = request.ExamId,
                Part = 6,
                QuestionNumber = qDto.QuestionNumber,
                QuestionText = qDto.QuestionText?.Trim(),
                CorrectOption = correctOptionEnum,
                Passage = passage
            };

            foreach (var optDto in qDto.Options)
            {
                Enum.TryParse<OptionLetter>(optDto.Letter, true, out var optLetterEnum);
                question.QuestionOptions.Add(new QuestionOption
                {
                    OptionLetter = optLetterEnum,
                    OptionText = optDto.Text.Trim()
                });
            }

            context.Questions.Add(question);
        }

        var success = await context.SaveChangesAsync(cancellationToken) > 0;

        if (!success)
        {
            return Result<int>.Failure("Could not save the new text completion set to the database.");
        }

        return Result<int>.Success(passage.Id);
    }
}
