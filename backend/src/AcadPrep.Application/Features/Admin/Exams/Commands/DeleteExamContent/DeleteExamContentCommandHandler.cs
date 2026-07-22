using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using AcadPrep.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.DeleteExamContent;

internal sealed class DeleteExamContentCommandHandler(IAppDbContext context)
    : IRequestHandler<DeleteExamContentCommand, Result<DeleteExamContentResultDto>>
{
    public async Task<Result<DeleteExamContentResultDto>> Handle(
        DeleteExamContentCommand request, CancellationToken cancellationToken)
    {
        var examExists = await context.Exams.AnyAsync(e => e.Id == request.ExamId && !e.IsDeleted, cancellationToken);
        if (!examExists)
        {
            return Result<DeleteExamContentResultDto>.Failure("Exam not found or has been deleted.");
        }

        // Guard against exam attempts
        var hasAttempts = await context.ExamAttempts.AnyAsync(a => a.ExamId == request.ExamId, cancellationToken);
        if (hasAttempts)
        {
            return Result<DeleteExamContentResultDto>.Failure("Cannot modify or delete content because this exam has existing student attempts.");
        }

        var result = new DeleteExamContentResultDto();

        if (TryGetStandaloneQuestionPart(request.ContentType, out var standalonePart))
        {
            var question = await context.Questions
                .Include(q => q.QuestionOptions)
                .FirstOrDefaultAsync(
                    q => q.Id == request.TargetId && q.ExamId == request.ExamId && q.Part == standalonePart,
                    cancellationToken);

            if (question == null)
            {
                return Result<DeleteExamContentResultDto>.Failure($"Part {standalonePart} question not found.");
            }

            result.DeletedQuestionIds.Add(question.Id);
            context.QuestionOptions.RemoveRange(question.QuestionOptions);
            context.Questions.Remove(question);
        }
        else if (request.ContentType.Equals("question", System.StringComparison.OrdinalIgnoreCase))
        {
            var question = await context.Questions
                .Include(q => q.QuestionOptions)
                .FirstOrDefaultAsync(q => q.Id == request.TargetId && q.ExamId == request.ExamId, cancellationToken);

            if (question == null)
            {
                return Result<DeleteExamContentResultDto>.Failure("Question not found.");
            }

            result.DeletedQuestionIds.Add(question.Id);
            context.QuestionOptions.RemoveRange(question.QuestionOptions);
            context.Questions.Remove(question);
        }
        else if (request.ContentType.Equals("listening", System.StringComparison.OrdinalIgnoreCase))
        {
            var group = await context.QuestionGroups
                .Include(g => g.Questions)
                    .ThenInclude(q => q.QuestionOptions)
                .FirstOrDefaultAsync(g => g.Id == request.TargetId && g.ExamId == request.ExamId, cancellationToken);

            if (group == null)
            {
                return Result<DeleteExamContentResultDto>.Failure("Listening group not found.");
            }

            result.DeletedQuestionGroupId = group.Id;
            foreach (var question in group.Questions)
            {
                result.DeletedQuestionIds.Add(question.Id);
                context.QuestionOptions.RemoveRange(question.QuestionOptions);
                context.Questions.Remove(question);
            }
            context.QuestionGroups.Remove(group);
        }
        else if (request.ContentType.Equals("textcompletion", System.StringComparison.OrdinalIgnoreCase))
        {
            var passage = await context.Passages
                .Include(p => p.Questions)
                    .ThenInclude(q => q.QuestionOptions)
                .FirstOrDefaultAsync(p => p.Id == request.TargetId && p.ExamId == request.ExamId, cancellationToken);

            if (passage == null)
            {
                return Result<DeleteExamContentResultDto>.Failure("Text completion passage not found.");
            }

            result.DeletedPassageIds.Add(passage.Id);
            foreach (var question in passage.Questions)
            {
                result.DeletedQuestionIds.Add(question.Id);
                context.QuestionOptions.RemoveRange(question.QuestionOptions);
                context.Questions.Remove(question);
            }
            context.Passages.Remove(passage);
        }
        else if (request.ContentType.Equals("readingset", System.StringComparison.OrdinalIgnoreCase))
        {
            var group = await context.QuestionGroups
                .Include(g => g.Passages)
                .Include(g => g.Questions)
                    .ThenInclude(q => q.QuestionOptions)
                .FirstOrDefaultAsync(g => g.Id == request.TargetId && g.ExamId == request.ExamId, cancellationToken);

            if (group == null)
            {
                return Result<DeleteExamContentResultDto>.Failure("Reading set group not found.");
            }

            result.DeletedQuestionGroupId = group.Id;
            foreach (var p in group.Passages)
            {
                result.DeletedPassageIds.Add(p.Id);
                context.Passages.Remove(p);
            }
            foreach (var question in group.Questions)
            {
                result.DeletedQuestionIds.Add(question.Id);
                context.QuestionOptions.RemoveRange(question.QuestionOptions);
                context.Questions.Remove(question);
            }
            context.QuestionGroups.Remove(group);
        }
        else
        {
            return Result<DeleteExamContentResultDto>.Failure("Invalid content type for deletion.");
        }

        var success = await context.SaveChangesAsync(cancellationToken) > 0;
        if (!success)
        {
            return Result<DeleteExamContentResultDto>.Failure("Could not delete the requested content from the database.");
        }

        return Result<DeleteExamContentResultDto>.Success(result);
    }

    private static bool TryGetStandaloneQuestionPart(string contentType, out int part)
    {
        part = contentType.ToLowerInvariant() switch
        {
            "part1" => 1,
            "part2" => 2,
            "part5" => 5,
            _ => 0
        };
        return part > 0;
    }
}
