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

namespace AcadPrep.Application.Features.Admin.Exams.Commands.CreateListeningGroup;

internal sealed class CreateListeningGroupCommandHandler(IAppDbContext context)
    : IRequestHandler<CreateListeningGroupCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateListeningGroupCommand request, CancellationToken cancellationToken)
    {
        var exam = await context.Exams
            .FirstOrDefaultAsync(x => x.Id == request.ExamId && !x.IsDeleted, cancellationToken);

        if (exam == null)
        {
            return Result<int>.Failure("Exam not found or has been deleted.");
        }

        var dto = request.Group;

        if (dto.Part is < 3 or > 4)
        {
            return Result<int>.Failure("Listening groups are only supported for Part 3 and Part 4.");
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

        if (dto.Questions.Count != ToeicPartLimits.ListeningGroupQuestionCount)
        {
            return Result<int>.Failure("Part 3 and Part 4 listening groups must have exactly 3 questions.");
        }

        var partCount = await context.Questions
            .CountAsync(q => q.ExamId == request.ExamId && q.Part == dto.Part, cancellationToken);

        if (!ToeicPartLimits.CanAddQuestionCount(dto.Part, partCount, ToeicPartLimits.ListeningGroupQuestionCount))
        {
            return Result<int>.Failure(
                $"Part {dto.Part} already has the maximum of {ToeicPartLimits.GetLimit(dto.Part)} questions.");
        }

        // Check if any question numbers are already taken in the exam
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

        // Validate questions correct answers and options count
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

        var group = new QuestionGroup
        {
            ExamId = request.ExamId,
            Name = dto.Name.Trim(),
            ImageUrl = dto.Media.ImageUrl?.Trim(),
            AudioUrl = !dto.Media.UseExamFullAudio ? dto.Media.AudioUrl?.Trim() : null,
            AudioStartSecond = dto.Media.UseExamFullAudio ? dto.Media.AudioStartSecond : null,
            AudioEndSecond = dto.Media.UseExamFullAudio ? dto.Media.AudioEndSecond : null
        };

        context.QuestionGroups.Add(group);

        // Add questions to group
        foreach (var qDto in dto.Questions)
        {
            Enum.TryParse<OptionLetter>(qDto.CorrectOption, true, out var correctOptionEnum);
            var question = new Question
            {
                ExamId = request.ExamId,
                Part = dto.Part,
                QuestionNumber = qDto.QuestionNumber,
                QuestionText = qDto.QuestionText?.Trim(),
                ImageUrl = qDto.ImageUrl?.Trim(),
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
            return Result<int>.Failure("Could not save the new listening group to the database.");
        }

        return Result<int>.Success(group.Id);
    }
}
