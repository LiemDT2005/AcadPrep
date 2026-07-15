using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcadPrep.Application.Common.Models;
using AcadPrep.Application.Common.Utils;
using Application.Common.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.UpdateListeningGroup;

internal sealed class UpdateListeningGroupCommandHandler(IAppDbContext context)
    : IRequestHandler<UpdateListeningGroupCommand, Result<int>>
{
    public async Task<Result<int>> Handle(UpdateListeningGroupCommand request, CancellationToken cancellationToken)
    {
        var exam = await context.Exams
            .FirstOrDefaultAsync(x => x.Id == request.ExamId && !x.IsDeleted, cancellationToken);

        if (exam == null)
        {
            return Result<int>.Failure("Exam not found or has been deleted.");
        }

        var hasAttempts = await context.ExamAttempts.AnyAsync(a => a.ExamId == request.ExamId, cancellationToken);
        if (hasAttempts)
        {
            return Result<int>.Failure("Cannot modify content because this exam has existing student attempts.");
        }

        var dto = request.Group;
        if (dto.Part is < 3 or > 4)
        {
            return Result<int>.Failure("Listening groups are only supported for Part 3 and Part 4.");
        }

        var group = await context.QuestionGroups
            .Include(g => g.Questions)
                .ThenInclude(q => q.QuestionOptions)
            .FirstOrDefaultAsync(
                g => g.Id == request.QuestionGroupId && g.ExamId == request.ExamId,
                cancellationToken);

        if (group == null)
        {
            return Result<int>.Failure("Listening group not found.");
        }

        var existingQuestions = group.Questions.Where(q => q.Part == dto.Part).OrderBy(q => q.QuestionNumber).ToList();
        if (existingQuestions.Count != ToeicPartLimits.ListeningGroupQuestionCount
            || dto.Questions.Count != ToeicPartLimits.ListeningGroupQuestionCount)
        {
            return Result<int>.Failure("Part 3 and Part 4 listening groups must have exactly 3 questions.");
        }

        if (!string.IsNullOrWhiteSpace(exam.AudioUrl))
        {
            if (!dto.Media.UseExamFullAudio)
            {
                return Result<int>.Failure(
                    "This exam has a full audio file. Specify start/end seconds instead of uploading a separate audio file.");
            }

            if (!dto.Media.AudioStartSecond.HasValue || !dto.Media.AudioEndSecond.HasValue)
            {
                return Result<int>.Failure("Audio start and end seconds are required when using full exam audio.");
            }

            if (dto.Media.AudioStartSecond < 0 || dto.Media.AudioEndSecond <= dto.Media.AudioStartSecond)
            {
                return Result<int>.Failure("Invalid audio start/end range.");
            }
        }
        else if (dto.Media.UseExamFullAudio)
        {
            return Result<int>.Failure(
                "This exam does not have a full audio file. Upload exam audio or use a separate audio file for this group.");
        }
        else if (string.IsNullOrWhiteSpace(dto.Media.AudioUrl))
        {
            return Result<int>.Failure("Audio URL is required when not using full exam audio.");
        }

        var inputQuestionNumbers = dto.Questions.Select(q => q.QuestionNumber).ToList();
        if (inputQuestionNumbers.Distinct().Count() != dto.Questions.Count)
        {
            return Result<int>.Failure("Duplicate question numbers in payload.");
        }

        var existingIds = existingQuestions.Select(q => q.Id).ToHashSet();
        if (dto.Questions.Any(q => !existingIds.Contains(q.Id)))
        {
            return Result<int>.Failure("One or more questions do not belong to this listening group.");
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

        group.Name = dto.Name.Trim();
        group.ImageUrl = dto.Media.ImageUrl?.Trim();
        group.AudioUrl = !dto.Media.UseExamFullAudio ? dto.Media.AudioUrl?.Trim() : null;
        group.AudioStartSecond = dto.Media.UseExamFullAudio ? dto.Media.AudioStartSecond : null;
        group.AudioEndSecond = dto.Media.UseExamFullAudio ? dto.Media.AudioEndSecond : null;

        foreach (var qDto in dto.Questions)
        {
            var question = existingQuestions.First(q => q.Id == qDto.Id);
            Enum.TryParse<OptionLetter>(qDto.CorrectOption, true, out var correctOptionEnum);

            question.QuestionNumber = qDto.QuestionNumber;
            question.QuestionText = qDto.QuestionText?.Trim();
            question.ImageUrl = qDto.ImageUrl?.Trim();
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
            return Result<int>.Failure("Could not update the listening group.");
        }

        return Result<int>.Success(group.Id);
    }
}
