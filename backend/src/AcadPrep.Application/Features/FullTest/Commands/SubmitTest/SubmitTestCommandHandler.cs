using AcadPrep.Application.Common.Models;
using AcadPrep.Application.Common.Utils;
using Application.Common.Interfaces;
using Domain.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.FullTest.Commands.SubmitTest;

public class SubmitTestCommandHandler : IRequestHandler<SubmitTestCommand, Result<SubmitTestResultDto>>
{
    private readonly IAppDbContext _context;
    private readonly INotificationService _notificationService;

    public SubmitTestCommandHandler(IAppDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<Result<SubmitTestResultDto>> Handle(SubmitTestCommand request, CancellationToken cancellationToken)
    {
        var attempt = await _context.ExamAttempts
            .Include(a => a.AttemptAnswers)
            .FirstOrDefaultAsync(a => a.Id == request.AttemptId && a.UserId == request.UserId, cancellationToken);

        if (attempt is null)
        {
            return Result<SubmitTestResultDto>.Failure("Không tìm thấy phiên thi.");
        }

        if (attempt.IsSubmitted)
        {
            return Result<SubmitTestResultDto>.Success(new SubmitTestResultDto
            {
                AttemptId = attempt.Id,
                ListeningScore = attempt.ListeningScore,
                ReadingScore = attempt.ReadingScore,
                TotalScore = attempt.TotalScore
            });
        }

        if (request.RemainingSeconds.HasValue)
        {
            attempt.RemainingTime = Math.Max(0, request.RemainingSeconds.Value);
        }

        var questions = await _context.Questions
            .AsNoTracking()
            .Where(q => q.ExamId == attempt.ExamId)
            .Select(q => new { q.Id, q.Part, q.CorrectOption })
            .ToListAsync(cancellationToken);

        var answerMap = attempt.AttemptAnswers
            .Where(a => a.SelectedOption.HasValue)
            .ToDictionary(a => a.QuestionId, a => a);

        var listeningCorrect = 0;
        var readingCorrect = 0;

        foreach (var q in questions)
        {
            if (!answerMap.TryGetValue(q.Id, out var ans) || !ans.SelectedOption.HasValue)
            {
                continue;
            }

            var isCorrect = ans.SelectedOption.Value == q.CorrectOption;
            ans.IsCorrect = isCorrect;

            if (q.Part <= 4 && isCorrect) listeningCorrect++;
            if (q.Part >= 5 && isCorrect) readingCorrect++;
        }

        var listeningScore = ToeicScoreConverter.CalculateListeningScore(listeningCorrect);
        var readingScore = ToeicScoreConverter.CalculateReadingScore(readingCorrect);

        attempt.ListeningScore = listeningScore;
        attempt.ReadingScore = readingScore;
        attempt.TotalScore = listeningScore + readingScore;
        attempt.IsSubmitted = true;
        attempt.CompletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Thông báo kết quả bài thi cho học viên (UC-15)
        var examTitle = await _context.Exams
            .Where(e => e.Id == attempt.ExamId)
            .Select(e => e.Title)
            .FirstOrDefaultAsync(cancellationToken) ?? "bài thi";

        await _notificationService.CreateAsync(
            userId: attempt.UserId,
            title: "Kết quả bài thi đã sẵn sàng",
            message: $"Bài thi '{examTitle}' đã được chấm xong. Điểm của bạn: {attempt.TotalScore}/990. Nhấn để xem chi tiết.",
            type: NotificationType.ExamResultReady,
            linkUrl: $"/Exams/Results?attemptId={attempt.Id}",
            cancellationToken: cancellationToken);

        return Result<SubmitTestResultDto>.Success(new SubmitTestResultDto
        {
            AttemptId = attempt.Id,
            ListeningScore = listeningScore,
            ReadingScore = readingScore,
            TotalScore = listeningScore + readingScore,
            ListeningCorrect = listeningCorrect,
            ReadingCorrect = readingCorrect
        });
    }
}
