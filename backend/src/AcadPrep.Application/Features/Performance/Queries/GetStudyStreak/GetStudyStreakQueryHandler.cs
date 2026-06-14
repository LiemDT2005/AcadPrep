using System;
using System.Threading;
using System.Threading.Tasks;
using AcadPrep.Application.Features.Performance.DTOs;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Performance.Queries.GetStudyStreak;

public class GetStudyStreakQueryHandler : IRequestHandler<GetStudyStreakQuery, StudyStreakDto>
{
    private readonly IAppDbContext _context;

    public GetStudyStreakQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<StudyStreakDto> Handle(GetStudyStreakQuery request, CancellationToken cancellationToken)
    {
        var streak = await _context.StudyStreaks
            .FirstOrDefaultAsync(s => s.UserId == request.UserId, cancellationToken);

        if (streak == null)
        {
            return new StudyStreakDto
            {
                CurrentStreak = 0,
                MaxStreak = 0,
                LastActiveDate = DateOnly.FromDateTime(DateTime.UtcNow)
            };
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var yesterday = today.AddDays(-1);

        // Reset streak to zero if LastActiveDate is less than yesterday
        if (streak.LastActiveDate < yesterday)
        {
            streak.CurrentStreak = 0;
            await _context.SaveChangesAsync(cancellationToken);
        }

        return new StudyStreakDto
        {
            CurrentStreak = streak.CurrentStreak,
            MaxStreak = streak.MaxStreak,
            LastActiveDate = streak.LastActiveDate
        };
    }
}
