using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Performance.Commands.ResetStudyStreak;

public class ResetStudyStreakHandler : IRequestHandler<ResetStudyStreakCommand>
{
    private readonly IAppDbContext _context;

    public ResetStudyStreakHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ResetStudyStreakCommand request, CancellationToken cancellationToken)
    {
        var streak = await _context.StudyStreaks
            .FirstOrDefaultAsync(s => s.UserId == request.UserId, cancellationToken);

        if (streak != null)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var yesterday = today.AddDays(-1);

            if (streak.LastActiveDate < yesterday)
            {
                streak.CurrentStreak = 0;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
