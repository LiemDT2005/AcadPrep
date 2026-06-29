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

namespace AcadPrep.Application.Features.Performance.Queries.GetLeaderboard;

public class GetLeaderboardQueryHandler : IRequestHandler<GetLeaderboardQuery, Result<LeaderboardResultDto>>
{
    private readonly IAppDbContext _context;

    public GetLeaderboardQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<LeaderboardResultDto>> Handle(GetLeaderboardQuery request, CancellationToken cancellationToken)
    {
        var result = new LeaderboardResultDto();

        // Query all users with their statistics
        var usersStatsQuery = _context.Users
            .Select(u => new
            {
                User = u,
                TotalSum = _context.ExamAttempts.Where(ea => ea.UserId == u.Id && ea.IsSubmitted).Sum(ea => ea.TotalScore),
                ExamsDone = _context.ExamAttempts.Count(ea => ea.UserId == u.Id && ea.IsSubmitted),
                Streak = _context.StudyStreaks.Where(s => s.UserId == u.Id).Select(s => s.CurrentStreak).FirstOrDefault()
            });

        // Apply sorting based on request
        List<LeaderboardEntryDto> sortedList = new();
        
        if (string.Equals(request.SortBy, "Streak", StringComparison.OrdinalIgnoreCase))
        {
            var usersStats = await usersStatsQuery
                .OrderByDescending(x => x.Streak)
                .ThenByDescending(x => x.TotalSum)
                .ToListAsync(cancellationToken);

            int rank = 1;
            foreach (var stat in usersStats)
            {
                sortedList.Add(new LeaderboardEntryDto
                {
                    Rank = rank++,
                    Name = stat.User.FullName ?? stat.User.Email,
                    TotalScore = stat.TotalSum,
                    ExamsDone = stat.ExamsDone,
                    StreakDays = stat.Streak,
                    IsCurrentUser = stat.User.Id == request.UserId
                });
            }
        }
        else // Default to Score
        {
            var usersStats = await usersStatsQuery
                .OrderByDescending(x => x.TotalSum)
                .ThenByDescending(x => x.Streak)
                .ToListAsync(cancellationToken);

            int rank = 1;
            foreach (var stat in usersStats)
            {
                sortedList.Add(new LeaderboardEntryDto
                {
                    Rank = rank++,
                    Name = stat.User.FullName ?? stat.User.Email,
                    TotalScore = stat.TotalSum,
                    ExamsDone = stat.ExamsDone,
                    StreakDays = stat.Streak,
                    IsCurrentUser = stat.User.Id == request.UserId
                });
            }
        }

        result.Entries = sortedList;
        result.CurrentUserEntry = sortedList.FirstOrDefault(e => e.IsCurrentUser) ?? new LeaderboardEntryDto();

        return result;
    }
}
