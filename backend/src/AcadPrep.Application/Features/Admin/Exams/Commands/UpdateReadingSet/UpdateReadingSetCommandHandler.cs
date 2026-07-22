using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcadPrep.Application.Common.Models;
using Application.Common.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.UpdateReadingSet;

internal sealed class UpdateReadingSetCommandHandler(IAppDbContext context)
    : IRequestHandler<UpdateReadingSetCommand, Result<int>>
{
    public async Task<Result<int>> Handle(UpdateReadingSetCommand request, CancellationToken cancellationToken)
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

        var group = await context.QuestionGroups
            .Include(g => g.Passages)
            .Include(g => g.Questions)
                .ThenInclude(q => q.QuestionOptions)
            .FirstOrDefaultAsync(
                g => g.Id == request.QuestionGroupId && g.ExamId == request.ExamId,
                cancellationToken);

        if (group == null)
        {
            return Result<int>.Failure("Reading set not found.");
        }

        var dto = request.Set;
        var existingPassages = group.Passages.OrderBy(p => p.DisplayOrder).ToList();
        var existingQuestions = group.Questions.Where(q => q.Part == 7).OrderBy(q => q.QuestionNumber).ToList();

        if (existingPassages.Count != dto.Passages.Count)
        {
            return Result<int>.Failure("Cannot change the number of passages when editing. Delete and recreate the set if needed.");
        }

        if (existingQuestions.Count != dto.Questions.Count)
        {
            return Result<int>.Failure("Cannot change the number of questions when editing. Delete and recreate the set if needed.");
        }

        var passageIds = existingPassages.Select(p => p.Id).ToHashSet();
        var questionIds = existingQuestions.Select(q => q.Id).ToHashSet();

        if (dto.Passages.Any(p => !passageIds.Contains(p.Id)) || dto.Questions.Any(q => !questionIds.Contains(q.Id)))
        {
            return Result<int>.Failure("One or more passages/questions do not belong to this reading set.");
        }

        foreach (var p in dto.Passages)
        {
            if (string.IsNullOrWhiteSpace(p.Content) && string.IsNullOrWhiteSpace(p.ImageUrl))
            {
                return Result<int>.Failure($"Passage at order {p.DisplayOrder} must have either content text or an image URL.");
            }
        }

        var inputQuestionNumbers = dto.Questions.Select(q => q.QuestionNumber).ToList();
        if (inputQuestionNumbers.Distinct().Count() != dto.Questions.Count)
        {
            return Result<int>.Failure("Duplicate question numbers in payload.");
        }

        var otherTaken = await context.Questions
            .Where(q => q.ExamId == request.ExamId && !questionIds.Contains(q.Id))
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

        group.Name = dto.Name.Trim();
        group.Explanation = string.IsNullOrWhiteSpace(dto.Explanation) ? null : dto.Explanation.Trim();

        foreach (var pDto in dto.Passages)
        {
            var passage = existingPassages.First(p => p.Id == pDto.Id);
            passage.Content = pDto.Content?.Trim();
            passage.ImageUrl = pDto.ImageUrl?.Trim();
            passage.DisplayOrder = pDto.DisplayOrder;
        }

        foreach (var qDto in dto.Questions)
        {
            var question = existingQuestions.First(q => q.Id == qDto.Id);
            Enum.TryParse<OptionLetter>(qDto.CorrectOption, true, out var correctOptionEnum);

            question.QuestionNumber = qDto.QuestionNumber;
            question.QuestionText = qDto.QuestionText.Trim();
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
            return Result<int>.Failure("Could not update the reading set.");
        }

        return Result<int>.Success(group.Id);
    }
}
