using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using AcadPrep.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Admin.Commands.DeleteAchievement;

public class DeleteAchievementCommandHandler : IRequestHandler<DeleteAchievementCommand, Result<bool>>
{
    private readonly IAppDbContext _context;

    public DeleteAchievementCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(DeleteAchievementCommand request, CancellationToken cancellationToken)
    {
        var achievement = await _context.Achievements.FirstOrDefaultAsync(a => a.AchievementId == request.AchievementId, cancellationToken);
        if (achievement == null)
        {
            return Result<bool>.Failure("Achievement not found");
        }

        _context.Achievements.Remove(achievement);
        await _context.SaveChangesAsync(cancellationToken);
        
        return Result<bool>.Success(true);
    }
}
