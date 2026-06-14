using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcadPrep.Application.Features.Performance.DTOs;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Performance.Queries.GetAchievements;

public class GetAchievementsQueryHandler : IRequestHandler<GetAchievementsQuery, List<AchievementDto>>
{
    private readonly IAppDbContext _context;

    public GetAchievementsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<AchievementDto>> Handle(GetAchievementsQuery request, CancellationToken cancellationToken)
    {
        var allAchievements = await _context.Achievements
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var userAchievements = await _context.UserAchievements
            .AsNoTracking()
            .Where(ua => ua.UserId == request.UserId)
            .ToDictionaryAsync(ua => ua.AchievementId, ua => ua.UnlockedAt, cancellationToken);

        var result = allAchievements.Select(a => new AchievementDto
        {
            AchievementId = a.AchievementId,
            Name = a.Name,
            Description = a.Description,
            IconUrl = a.IconUrl,
            IsUnlocked = userAchievements.ContainsKey(a.AchievementId),
            UnlockedAt = userAchievements.ContainsKey(a.AchievementId) ? userAchievements[a.AchievementId] : null
        })
        .OrderByDescending(a => a.IsUnlocked)
        .ThenBy(a => a.AchievementId)
        .ToList();

        return result;
    }
}
