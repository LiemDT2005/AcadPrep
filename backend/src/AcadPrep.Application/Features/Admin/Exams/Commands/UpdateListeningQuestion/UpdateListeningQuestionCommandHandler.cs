using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcadPrep.Application.Common.Models;
using Application.Common.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.UpdateListeningQuestion;

internal sealed class UpdateListeningQuestionCommandHandler(IAppDbContext context)
    : IRequestHandler<UpdateListeningQuestionCommand, Result<int>>
{
    public async Task<Result<int>> Handle(UpdateListeningQuestionCommand request, CancellationToken cancellationToken)
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

        var hasAttempts = await context.ExamAttempts.AnyAsync(a => a.ExamId == request.ExamId, cancellationToken);
        if (hasAttempts)
        {
            return Result<int>.Failure("Cannot modify content because this exam has existing student attempts.");
        }

        var question = await context.Questions
            .Include(q => q.QuestionOptions)
            .FirstOrDefaultAsync(
                q => q.Id == request.QuestionId && q.ExamId == request.ExamId && q.Part == request.Part,
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

        if (!System.Enum.TryParse<OptionLetter>(dto.CorrectOption, true, out var correctOptionEnum)
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

        question.QuestionNumber = dto.QuestionNumber;
        question.QuestionText = dto.QuestionText?.Trim();
        question.ImageUrl = dto.ImageUrl?.Trim();
        question.CorrectOption = correctOptionEnum;
        question.Explanation = string.IsNullOrWhiteSpace(dto.Explanation) ? null : dto.Explanation.Trim();
        question.AudioUrl = !dto.UseExamFullAudio ? dto.AudioUrl?.Trim() : null;
        question.AudioStartSecond = dto.UseExamFullAudio ? dto.AudioStartSecond : null;
        question.AudioEndSecond = dto.UseExamFullAudio ? dto.AudioEndSecond : null;

        context.QuestionOptions.RemoveRange(question.QuestionOptions);
        question.QuestionOptions.Clear();

        foreach (var optDto in dto.Options)
        {
            System.Enum.TryParse<OptionLetter>(optDto.Letter, true, out var optLetterEnum);
            question.QuestionOptions.Add(new Domain.Entities.QuestionOption
            {
                OptionLetter = optLetterEnum,
                OptionText = optDto.Text.Trim()
            });
        }

        var success = await context.SaveChangesAsync(cancellationToken) > 0;
        if (!success)
        {
            return Result<int>.Failure("Could not update the listening question.");
        }

        return Result<int>.Success(question.Id);
    }
}
