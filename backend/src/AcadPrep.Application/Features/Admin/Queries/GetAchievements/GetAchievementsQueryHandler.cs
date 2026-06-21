using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using AcadPrep.Application.Common.Models;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Admin.Queries.GetAchievements;

public class GetAchievementsQueryHandler : IRequestHandler<GetAchievementsQuery, Result<List<Achievement>>>
{
    private readonly IAppDbContext _context;

    public GetAchievementsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<Achievement>>> Handle(GetAchievementsQuery request, CancellationToken cancellationToken)
    {
        var achievements = await _context.Achievements
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        
        return Result<List<Achievement>>.Success(achievements);
    }
}
