using AcadPrep.Application.Common.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcadPrep.Application.Features.Admin.DTOs;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Admin.Queries.GetUserStats;

public class GetUserStatsQueryHandler : IRequestHandler<GetUserStatsQuery, Result<UserStatsDto>>
{
    private readonly IAppDbContext _context;

    public GetUserStatsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<UserStatsDto>> Handle(GetUserStatsQuery request, CancellationToken cancellationToken)
    {
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var sevenDaysAgo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7));

        var totalUsers = await _context.Users.CountAsync(cancellationToken);
        
        var newRegistrations = await _context.Users
            .CountAsync(u => u.CreatedAt >= thirtyDaysAgo, cancellationToken);
            
        var activeUsers = await _context.StudyStreaks
            .CountAsync(s => s.LastActiveDate >= sevenDaysAgo, cancellationToken);

        var totalExamsTaken = await _context.ExamAttempts
            .CountAsync(ea => ea.IsSubmitted, cancellationToken);

        var avgScore = 0.0;
        if (totalExamsTaken > 0)
        {
            avgScore = await _context.ExamAttempts
                .Where(ea => ea.IsSubmitted)
                .AverageAsync(ea => ea.TotalScore, cancellationToken);
        }

        return new UserStatsDto
        {
            TotalUsers = totalUsers,
            NewRegistrations = newRegistrations,
            ActiveUsers = activeUsers,
            TotalExamsTaken = totalExamsTaken,
            AverageToeicScore = avgScore
        };
    }
}

