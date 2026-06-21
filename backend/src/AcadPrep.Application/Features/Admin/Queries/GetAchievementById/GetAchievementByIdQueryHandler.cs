using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using AcadPrep.Application.Common.Models;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Admin.Queries.GetAchievementById;

public class GetAchievementByIdQueryHandler : IRequestHandler<GetAchievementByIdQuery, Result<Achievement>>
{
    private readonly IAppDbContext _context;

    public GetAchievementByIdQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Achievement>> Handle(GetAchievementByIdQuery request, CancellationToken cancellationToken)
    {
        var achievement = await _context.Achievements
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AchievementId == request.AchievementId, cancellationToken);
        
        if (achievement == null)
        {
            return Result<Achievement>.Failure("Achievement not found");
        }
        
        return Result<Achievement>.Success(achievement);
    }
}
