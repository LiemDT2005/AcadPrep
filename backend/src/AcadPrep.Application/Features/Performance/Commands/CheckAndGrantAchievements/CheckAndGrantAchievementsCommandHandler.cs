using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using AcadPrep.Application.Common.Models;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Performance.Commands.CheckAndGrantAchievements;

public class CheckAndGrantAchievementsCommandHandler : IRequestHandler<CheckAndGrantAchievementsCommand, Result<bool>>
{
    private readonly IAppDbContext _context;

    public CheckAndGrantAchievementsCommandHandler(IAppDbContext context)
    {
        _context = context;
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
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}
