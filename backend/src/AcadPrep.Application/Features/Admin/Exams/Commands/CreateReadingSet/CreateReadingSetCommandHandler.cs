using System;
using System.Collections.Generic;
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

namespace AcadPrep.Application.Features.Admin.Exams.Commands.CreateReadingSet;

internal sealed class CreateReadingSetCommandHandler(IAppDbContext context)
    : IRequestHandler<CreateReadingSetCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateReadingSetCommand request, CancellationToken cancellationToken)
    {
        var exam = await context.Exams
            .FirstOrDefaultAsync(x => x.Id == request.ExamId && !x.IsDeleted, cancellationToken);

        if (exam == null)
        {
            return Result<int>.Failure("Exam not found or has been deleted.");
        }

        var dto = request.Set;

        if (dto.Passages.Count < 1 || dto.Passages.Count > 3)
        {
            return Result<int>.Failure("A Part 7 reading set must have between 1 and 3 passages.");
        }

        foreach (var p in dto.Passages)
        {
            if (string.IsNullOrWhiteSpace(p.Content) && string.IsNullOrWhiteSpace(p.ImageUrl))
            {
                return Result<int>.Failure($"Passage at order {p.DisplayOrder} must have either content text or an image URL.");
            }
        }

        if (dto.Questions.Count < ToeicPartLimits.ReadingSetMinQuestionCount || dto.Questions.Count > 5)
        {
            return Result<int>.Failure("A Part 7 reading set must have between 2 and 5 questions.");
        }

        var part7Count = await context.Questions
            .CountAsync(q => q.ExamId == request.ExamId && q.Part == 7, cancellationToken);

        if (!ToeicPartLimits.CanAddQuestionCount(7, part7Count, dto.Questions.Count))
        {
            return Result<int>.Failure(
                $"Part 7 already has the maximum of {ToeicPartLimits.GetLimit(7)} questions.");
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

        // Create QuestionGroup
        var group = new QuestionGroup
        {
            ExamId = request.ExamId,
            Name = dto.Name.Trim()
        };

        context.QuestionGroups.Add(group);

        foreach (var pDto in dto.Passages)
        {
            var passage = new Passage
            {
                ExamId = request.ExamId,
                Content = pDto.Content?.Trim(),
                ImageUrl = pDto.ImageUrl?.Trim(),
                DisplayOrder = pDto.DisplayOrder,
                QuestionGroup = group
            };
            context.Passages.Add(passage);
        }

        foreach (var qDto in dto.Questions)
        {
            Enum.TryParse<OptionLetter>(qDto.CorrectOption, true, out var correctOptionEnum);

            var question = new Question
            {
                ExamId = request.ExamId,
                Part = 7,
                QuestionNumber = qDto.QuestionNumber,
                QuestionText = qDto.QuestionText.Trim(),
                CorrectOption = correctOptionEnum,
                QuestionGroup = group
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
            return Result<int>.Failure("Could not save the new reading set to the database.");
        }

        return Result<int>.Success(group.Id);
    }
}
