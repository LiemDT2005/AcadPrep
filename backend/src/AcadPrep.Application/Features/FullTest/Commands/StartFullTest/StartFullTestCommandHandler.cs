using AcadPrep.Application.Common.Models;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.FullTest.Commands.StartFullTest;

public class StartFullTestCommandHandler : IRequestHandler<StartFullTestCommand, Result<StartFullTestResultDto>>
{
    private readonly IAppDbContext _context;

    public StartFullTestCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<StartFullTestResultDto>> Handle(StartFullTestCommand request, CancellationToken cancellationToken)
    {
        var exam = await _context.Exams
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.ExamId && !e.IsDeleted, cancellationToken);

        if (exam is null)
        {
            return Result<StartFullTestResultDto>.Failure("Exam not found or has been deleted.");
        }

        var inProgress = await _context.ExamAttempts
            .FirstOrDefaultAsync(a =>
                a.ExamId == request.ExamId &&
                a.UserId == request.UserId &&
                !a.IsSubmitted, cancellationToken);

        if (inProgress is not null)
        {
            return Result<StartFullTestResultDto>.Failure(
                $"You have an unfinished test ({TimeSpan.FromSeconds(inProgress.RemainingTime):hh\\:mm\\:ss} remaining).");
        }

        var hasQuestions = await _context.Questions
            .AnyAsync(q => q.ExamId == request.ExamId, cancellationToken);

        if (!hasQuestions)
        {
            return Result<StartFullTestResultDto>.Failure("This exam has no questions yet. Cannot start a full test.");
        }

        var attempt = new ExamAttempt
        {
            UserId = request.UserId,
            ExamId = request.ExamId,
            RemainingTime = exam.Duration * 60,
            IsSubmitted = false,
            StartedAt = DateTime.UtcNow
        };

        _context.ExamAttempts.Add(attempt);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<StartFullTestResultDto>.Success(new StartFullTestResultDto
        {
            AttemptId = attempt.Id,
            IsResume = false
        });
    }
}
