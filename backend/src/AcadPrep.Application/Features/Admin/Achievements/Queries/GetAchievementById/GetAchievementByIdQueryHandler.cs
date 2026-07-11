using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using AcadPrep.Application.Common.Models;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

using AcadPrep.Application.Features.Admin.Achievements.DTOs;

namespace AcadPrep.Application.Features.Admin.Achievements.Queries.GetAchievementById;

public class GetAchievementByIdQueryHandler : IRequestHandler<GetAchievementByIdQuery, Result<AchievementAdminDto>>
{
    private readonly IAppDbContext _context;

    public GetAchievementByIdQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<AchievementAdminDto>> Handle(GetAchievementByIdQuery request, CancellationToken cancellationToken)
    {
        var achievement = await _context.Achievements
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AchievementId == request.AchievementId, cancellationToken);
        
        if (achievement == null)
        {
            return Result<AchievementAdminDto>.Failure("Achievement not found");
        }
        
        var dto = new AchievementAdminDto
        {
            AchievementId = achievement.AchievementId,
            Name = achievement.Name,
            Description = achievement.Description,
            IconUrl = achievement.IconUrl,
            ConditionType = achievement.ConditionType,
            ConditionValue = achievement.ConditionValue
        };

        return Result<AchievementAdminDto>.Success(dto);
    }
}
