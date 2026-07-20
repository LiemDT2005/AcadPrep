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

namespace AcadPrep.Application.Features.Admin.Exams.Commands.CreateListeningQuestion;

internal sealed class CreateListeningQuestionCommandHandler(IAppDbContext context)
    : IRequestHandler<CreateListeningQuestionCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateListeningQuestionCommand request, CancellationToken cancellationToken)
    {
        if (request.Part is not (1 or 2))
        {
            return Result<int>.Failure("Part must be 1 or 2 for standalone listening questions.");
        }

        var exam = await context.Exams
            .FirstOrDefaultAsync(x => x.Id == request.ExamId && !x.IsDeleted, cancellationToken);

        if (exam == null)
        {
            return Result<int>.Failure("Exam not found or has been deleted.");
        }

        var dto = request.Question;

        var partCount = await context.Questions
            .CountAsync(q => q.ExamId == request.ExamId && q.Part == request.Part, cancellationToken);

        if (ToeicPartLimits.IsPartFull(request.Part, partCount))
        {
            return Result<int>.Failure(
                $"Part {request.Part} already has the maximum of {ToeicPartLimits.GetLimit(request.Part)} questions.");
        }

        var questionNumberExists = await context.Questions
            .AnyAsync(q => q.ExamId == request.ExamId && q.QuestionNumber == dto.QuestionNumber, cancellationToken);

        if (questionNumberExists)
        {
            return Result<int>.Failure($"Question number {dto.QuestionNumber} already exists in this exam.");
        }

        if (request.Part == 1 && string.IsNullOrWhiteSpace(dto.ImageUrl))
        {
            return Result<int>.Failure("Part 1: A photograph image is required.");
        }

        if (!string.IsNullOrWhiteSpace(exam.AudioUrl))
        {
            if (!dto.UseExamFullAudio)
            {
                return Result<int>.Failure(
                    "This exam has a full audio file. Specify start/end seconds instead of uploading a separate audio file.");
            }

            if (!dto.AudioStartSecond.HasValue || !dto.AudioEndSecond.HasValue)
            {
                return Result<int>.Failure("Audio start and end seconds are required when using full exam audio.");
            }

            if (dto.AudioStartSecond < 0 || dto.AudioEndSecond <= dto.AudioStartSecond)
            {
                return Result<int>.Failure("Invalid audio start/end range.");
            }
        }
        else if (dto.UseExamFullAudio)
        {
            return Result<int>.Failure(
                "This exam does not have a full audio file. Upload exam audio or use a separate audio file for this question.");
        }
        else if (string.IsNullOrWhiteSpace(dto.AudioUrl))
        {
            return Result<int>.Failure("Audio file is required.");
        }

        var expectedOptionCount = request.Part == 2 ? 3 : 4;
        var validLetters = request.Part == 2
            ? new[] { "A", "B", "C" }
            : new[] { "A", "B", "C", "D" };

        if (!Enum.TryParse<OptionLetter>(dto.CorrectOption, true, out var correctOptionEnum)
            || !validLetters.Contains(dto.CorrectOption.ToUpper()))
        {
            return Result<int>.Failure($"Invalid correct option: {dto.CorrectOption}. Must be {string.Join(", ", validLetters)}.");
        }

        if (dto.Options.Count != expectedOptionCount)
        {
            return Result<int>.Failure($"A Part {request.Part} question must have exactly {expectedOptionCount} options.");
        }

        var letters = dto.Options.Select(o => o.Letter.ToUpper()).ToList();
        if (letters.Distinct().Count() != expectedOptionCount || !letters.All(l => validLetters.Contains(l)))
        {
            return Result<int>.Failure($"Options must have unique letters {string.Join(", ", validLetters)}.");
        }

        var question = new Question
        {
            ExamId = request.ExamId,
            Part = request.Part,
            QuestionNumber = dto.QuestionNumber,
            QuestionText = dto.QuestionText?.Trim(),
            ImageUrl = dto.ImageUrl?.Trim(),
            CorrectOption = correctOptionEnum,
            QuestionGroupId = null,
            Explanation = string.IsNullOrWhiteSpace(dto.Explanation) ? null : dto.Explanation.Trim(),
            AudioUrl = !dto.UseExamFullAudio ? dto.AudioUrl?.Trim() : null,
            AudioStartSecond = dto.UseExamFullAudio ? dto.AudioStartSecond : null,
            AudioEndSecond = dto.UseExamFullAudio ? dto.AudioEndSecond : null
        };

        foreach (var optDto in dto.Options)
        {
            Enum.TryParse<OptionLetter>(optDto.Letter, true, out var optLetterEnum);
            question.QuestionOptions.Add(new QuestionOption
            {
                OptionLetter = optLetterEnum,
                OptionText = optDto.Text.Trim()
            });
        }

        context.Questions.Add(question);

        var success = await context.SaveChangesAsync(cancellationToken) > 0;

        if (!success)
        {
            return Result<int>.Failure("Could not save the new listening question to the database.");
        }

        return Result<int>.Success(question.Id);
    }
}
