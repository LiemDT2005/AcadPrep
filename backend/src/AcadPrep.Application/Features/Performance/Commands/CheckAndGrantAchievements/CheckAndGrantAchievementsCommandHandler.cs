using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using AcadPrep.Application.Common.Models;
using Domain.Constants;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Performance.Commands.CheckAndGrantAchievements;

public class CheckAndGrantAchievementsCommandHandler : IRequestHandler<CheckAndGrantAchievementsCommand, Result<bool>>
{
    private readonly IAppDbContext _context;
    private readonly INotificationService _notificationService;

    public CheckAndGrantAchievementsCommandHandler(IAppDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<Result<bool>> Handle(CheckAndGrantAchievementsCommand request, CancellationToken cancellationToken)
    {
        // 1. Get user's current stats
        var userExamsCount = await _context.ExamAttempts
            .Where(ea => ea.UserId == request.UserId && ea.IsSubmitted)
            .CountAsync(cancellationToken);
            
        var maxScore = await _context.ExamAttempts
            .Where(ea => ea.UserId == request.UserId && ea.IsSubmitted)
            .MaxAsync(ea => (int?)ea.TotalScore, cancellationToken) ?? 0;
            
        var vocabSavedCount = await _context.SavedVocabularies
            .Where(sv => sv.UserId == request.UserId)
            .CountAsync(cancellationToken);
            
        var studyStreak = await _context.StudyStreaks
            .FirstOrDefaultAsync(s => s.UserId == request.UserId, cancellationToken);
        var streakCount = studyStreak?.MaxStreak ?? 0;

        // 2. Get all achievements user hasn't unlocked yet
        var unlockedAchievementIds = await _context.UserAchievements
            .Where(ua => ua.UserId == request.UserId)
            .Select(ua => ua.AchievementId)
            .ToListAsync(cancellationToken);

        var lockedAchievements = await _context.Achievements
            .Where(a => !unlockedAchievementIds.Contains(a.AchievementId))
            .ToListAsync(cancellationToken);

        // 3. Evaluate each locked achievement
        var grantedAchievements = new List<Achievement>();
        foreach (var achievement in lockedAchievements)
        {
            bool isGranted = false;

            switch (achievement.ConditionType)
            {
                case "ExamsCompleted":
                    if (userExamsCount >= achievement.ConditionValue) isGranted = true;
                    break;
                case "Score":
                    if (maxScore >= achievement.ConditionValue) isGranted = true;
                    break;
                case "Streak":
                    if (streakCount >= achievement.ConditionValue) isGranted = true;
                    break;
                case "VocabCount":
                    if (vocabSavedCount >= achievement.ConditionValue) isGranted = true;
                    break;
                // Add more conditions here as needed
            }

            if (isGranted)
            {
                _context.UserAchievements.Add(new UserAchievement
                {
                    UserId = request.UserId,
                    AchievementId = achievement.AchievementId,
                    UnlockedAt = DateTime.UtcNow,
                    IsNotified = false // This will trigger the UI popup
                });
                grantedAchievements.Add(achievement);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Gửi thông báo cho mỗi danh hiệu vừa mở khóa (UC-15)
        foreach (var achievement in grantedAchievements)
        {
            await _notificationService.CreateAsync(
                userId: request.UserId,
                title: "Bạn vừa mở khóa danh hiệu mới!",
                message: $"Chúc mừng! Bạn đã đạt danh hiệu '{achievement.Name}'. {achievement.Description}",
                type: NotificationType.AchievementUnlocked,
                linkUrl: "/Performance/Achievements",
                cancellationToken: cancellationToken);
        }

        return Result<bool>.Success(true);
    }
}
