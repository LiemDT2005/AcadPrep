using AcadPrep.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcadPrep.Application.Features.Performance.DTOs;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Performance.Queries.GetDashboardData;

public class GetDashboardDataQueryHandler : IRequestHandler<GetDashboardDataQuery, Result<DashboardDataDto>>
{
    private readonly IAppDbContext _context;

    public GetDashboardDataQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<DashboardDataDto>> Handle(GetDashboardDataQuery request, CancellationToken cancellationToken)
    {
        var result = new DashboardDataDto();

        // 1. Active Exams
        var activeAttempts = await _context.ExamAttempts
            .Include(ea => ea.Exam)
            .Include(ea => ea.AttemptAnswers)
            .Where(ea => ea.UserId == request.UserId && !ea.IsSubmitted)
            .OrderByDescending(ea => ea.StartedAt)
            .ToListAsync(cancellationToken);

        foreach (var attempt in activeAttempts)
        {
            int totalQuestions = await _context.Questions.CountAsync(q => q.ExamId == attempt.ExamId, cancellationToken);
            int answered = attempt.AttemptAnswers.Count;
            int progress = totalQuestions > 0 ? (int)((double)answered / totalQuestions * 100) : 0;
            
            result.ActiveExams.Add(new ActiveExamDto
            {
                AttemptId = attempt.AttemptId,
                ExamTitle = attempt.Exam.Title ?? "Unknown Exam",
                StatusText = $"Còn {attempt.RemainingTime} phút, Đã làm {progress}%",
                ProgressPercentage = progress
            });
        }

        // 2. Recent Activities (Mocking mixing from ExamAttempts and SavedVocabs)
        var recentExams = await _context.ExamAttempts
            .Include(ea => ea.Exam)
            .Where(ea => ea.UserId == request.UserId && ea.IsSubmitted && ea.CompletedAt != null)
            .OrderByDescending(ea => ea.CompletedAt)
            .Take(3)
            .ToListAsync(cancellationToken);

        foreach (var exam in recentExams)
        {
            result.RecentActivities.Add(new ActivityLogDto
            {
                Description = $"Đã làm đề {exam.Exam.Title} - Đạt {exam.TotalScore} điểm.",
                CreatedAt = exam.CompletedAt.Value,
                ColorType = "primary",
                TimeAgo = GetTimeAgo(exam.CompletedAt.Value)
            });
        }

        var recentVocabs = await _context.SavedVocabularies
            .Where(v => v.UserId == request.UserId)
            .OrderByDescending(v => v.DateSaved)
            .Take(5)
            .ToListAsync(cancellationToken);
            
        if (recentVocabs.Any())
        {
            result.RecentActivities.Add(new ActivityLogDto
            {
                Description = $"Đã lưu {recentVocabs.Count} từ vựng mới.",
                CreatedAt = recentVocabs.First().DateSaved,
                ColorType = "tertiary",
                TimeAgo = GetTimeAgo(recentVocabs.First().DateSaved)
            });
        }

        result.RecentActivities = result.RecentActivities.OrderByDescending(a => a.CreatedAt).Take(5).ToList();

        // 3. Skill Analytics (Part 1-7)
        var userAnswers = await _context.AttemptAnswers
            .Include(a => a.Question)
            .Where(a => a.ExamAttempt.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        for (int p = 1; p <= 7; p++)
        {
            var partAnswers = userAnswers.Where(a => a.Question.Part == p).ToList();
            if (partAnswers.Any())
            {
                int correct = partAnswers.Count(a => a.IsCorrect);
                result.SkillAnalytics.PartMastery[p] = (int)((double)correct / partAnswers.Count * 100);
            }
            else
            {
                result.SkillAnalytics.PartMastery[p] = 0; // No data
            }
        }

        // 4. Leaderboard (Calculate sum of scores or max score. We'll use sum of top scores per exam or total score sum)
        // For simplicity, sum of TotalScore of submitted exams.
        var usersStats = await _context.Users
            .Select(u => new
            {
                User = u,
                TotalSum = _context.ExamAttempts.Where(ea => ea.UserId == u.UserId && ea.IsSubmitted).Sum(ea => ea.TotalScore),
                ExamsDone = _context.ExamAttempts.Count(ea => ea.UserId == u.UserId && ea.IsSubmitted),
                Streak = _context.StudyStreaks.Where(s => s.UserId == u.UserId).Select(s => s.CurrentStreak).FirstOrDefault()
            })
            .OrderByDescending(x => x.TotalSum)
            .ToListAsync(cancellationToken);

        int rank = 1;
        foreach (var stat in usersStats)
        {
            var entry = new LeaderboardEntryDto
            {
                Rank = rank,
                Name = stat.User.FullName ?? stat.User.Email,
                TotalScore = stat.TotalSum,
                ExamsDone = stat.ExamsDone,
                StreakDays = stat.Streak,
                IsCurrentUser = stat.User.UserId == request.UserId
            };

            if (rank <= 5) result.Leaderboard.TopUsers.Add(entry);
            if (entry.IsCurrentUser) result.Leaderboard.CurrentUser = entry;
            
            rank++;
        }

        return result;
    }

    private string GetTimeAgo(DateTime dt)
    {
        var span = DateTime.UtcNow - dt;
        if (span.Days > 0) return $"{span.Days} ngày trước";
        if (span.Hours > 0) return $"{span.Hours} giờ trước";
        if (span.Minutes > 0) return $"{span.Minutes} phút trước";
        return "Vừa xong";
    }
}
