using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using AcadPrep.Application.Common.Models;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

using AcadPrep.Application.Features.Admin.DTOs;
using System.Linq;

namespace AcadPrep.Application.Features.Admin.Queries.GetAchievements;

public class GetAchievementsQueryHandler : IRequestHandler<GetAchievementsQuery, Result<PaginatedList<AchievementAdminDto>>>
{
    private readonly IAppDbContext _context;

    public GetAchievementsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedList<AchievementAdminDto>>> Handle(GetAchievementsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Achievements
            .AsNoTracking()
            .Select(a => new AchievementAdminDto
            {
                AchievementId = a.AchievementId,
                Name = a.Name,
                Description = a.Description,
                IconUrl = a.IconUrl,
                ConditionType = a.ConditionType,
                ConditionValue = a.ConditionValue
            });
            
        var paginatedResult = await PaginatedList<AchievementAdminDto>.CreateAsync(
            query,
            request.PageNumber,
            request.PageSize
        );
        
        return Result<PaginatedList<AchievementAdminDto>>.Success(paginatedResult);
    }
}
