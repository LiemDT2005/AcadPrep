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

namespace AcadPrep.Application.Features.Admin.Exams.Commands.CreatePart5Question;

internal sealed class CreatePart5QuestionCommandHandler(IAppDbContext context)
    : IRequestHandler<CreatePart5QuestionCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreatePart5QuestionCommand request, CancellationToken cancellationToken)
    {
        var exam = await context.Exams
            .FirstOrDefaultAsync(x => x.Id == request.ExamId && !x.IsDeleted, cancellationToken);

        if (exam == null)
        {
            return Result<int>.Failure("Exam not found or has been deleted.");
        }

        var dto = request.Question;

        var partCount = await context.Questions
            .CountAsync(q => q.ExamId == request.ExamId && q.Part == 5, cancellationToken);

        if (ToeicPartLimits.IsPartFull(5, partCount))
        {
            return Result<int>.Failure(
                $"Part 5 already has the maximum of {ToeicPartLimits.GetLimit(5)} questions.");
        }

        // Check if question number already exists in this exam
        var questionNumberExists = await context.Questions
            .AnyAsync(q => q.ExamId == request.ExamId && q.QuestionNumber == dto.QuestionNumber, cancellationToken);

        if (questionNumberExists)
        {
            return Result<int>.Failure($"Question number {dto.QuestionNumber} already exists in this exam.");
        }

        // Parse CorrectOption
        if (!Enum.TryParse<OptionLetter>(dto.CorrectOption, true, out var correctOptionEnum))
        {
            return Result<int>.Failure($"Invalid correct option: {dto.CorrectOption}. Must be A, B, C, or D.");
        }

        // Validate options
        if (dto.Options.Count != 4)
        {
            return Result<int>.Failure("A question must have exactly 4 options.");
        }

        var letters = dto.Options.Select(o => o.Letter.ToUpper()).ToList();
        if (letters.Distinct().Count() != 4 || !letters.All(l => l == "A" || l == "B" || l == "C" || l == "D"))
        {
            return Result<int>.Failure("Options must have unique letters A, B, C, and D.");
        }

        var question = new Question
        {
            ExamId = request.ExamId,
            Part = 5,
            QuestionNumber = dto.QuestionNumber,
            QuestionText = dto.QuestionText?.Trim(),
            CorrectOption = correctOptionEnum,
            QuestionType = dto.QuestionType?.Trim(),
            TopicTag = dto.TopicTag?.Trim()
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
            return Result<int>.Failure("Could not save the new question to the database.");
        }

        return Result<int>.Success(question.Id);
    }
}
