using AcadPrep.Application.Common.Models;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.FullTest.Commands.StartFullTest;

public class StartFullTestCommandHandler : IRequestHandler<StartFullTestCommand, Result<StartFullTestResultDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICacheService _cache;
    private readonly IBillingAccessService _billing;

    public StartFullTestCommandHandler(IAppDbContext context, ICacheService cache, IBillingAccessService billing)
    {
        _context = context;
        _cache = cache;
        _billing = billing;
    }

    public async Task<Result<StartFullTestResultDto>> Handle(StartFullTestCommand request, CancellationToken cancellationToken)
    {
        var exam = await _context.Exams
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.ExamId && !e.IsDeleted && e.Status == ExamStatus.Published, cancellationToken);

        if (exam is null)
        {
            return Result<StartFullTestResultDto>.Failure("Exam not found or is not available.");
        }

        var inProgress = await _context.ExamAttempts
            .Include(a => a.AttemptAnswers)
            .FirstOrDefaultAsync(a =>
                a.ExamId == request.ExamId &&
                a.UserId == request.UserId &&
                !a.IsSubmitted, cancellationToken);

        if (inProgress is not null && !request.StartNewAttempt)
        {
            return Result<StartFullTestResultDto>.Failure(
                $"You have an unfinished test ({TimeSpan.FromSeconds(inProgress.RemainingTime):hh\\:mm\\:ss} remaining).");
        }

        // Gate freemium trước khi abandon / tạo attempt mới.
        var quota = await _billing.EnsureCanStartFullTestAsync(request.UserId, cancellationToken);
        if (!quota.Allowed)
        {
            return Result<StartFullTestResultDto>.Failure($"{quota.ErrorCode}|{quota.Message}");
        }

        int? abandonedAttemptId = null;
        if (inProgress is not null)
        {
            abandonedAttemptId = inProgress.Id;
            _context.AttemptAnswers.RemoveRange(inProgress.AttemptAnswers);
            _context.ExamAttempts.Remove(inProgress);
        }

        var hasQuestions = await _context.Questions
            .AnyAsync(q => q.ExamId == request.ExamId, cancellationToken);

        if (!hasQuestions)
        {
            return Result<StartFullTestResultDto>.Failure("This exam has no questions yet. Cannot start a full test.");
        }

        // Section timers: Listening 45 min first; Reading 75 min starts after listening ends.
        const int listeningSeconds = 45 * 60;

        var attempt = new ExamAttempt
        {
            UserId = request.UserId,
            ExamId = request.ExamId,
            RemainingTime = listeningSeconds,
            IsSubmitted = false,
            StartedAt = DateTime.UtcNow
        };

        _context.ExamAttempts.Add(attempt);
        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync($"ExamDetail_{request.ExamId}_U_{request.UserId}", cancellationToken);

        return Result<StartFullTestResultDto>.Success(new StartFullTestResultDto
        {
            AttemptId = attempt.Id,
            IsResume = false,
            AbandonedAttemptId = abandonedAttemptId
        });
    }
}
