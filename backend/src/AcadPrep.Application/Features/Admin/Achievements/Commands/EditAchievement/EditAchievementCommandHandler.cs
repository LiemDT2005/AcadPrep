using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using AcadPrep.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Admin.Achievements.Commands.EditAchievement;

public class EditAchievementCommandHandler : IRequestHandler<EditAchievementCommand, Result<int>>
{
    private readonly IAppDbContext _context;

    public EditAchievementCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(EditAchievementCommand request, CancellationToken cancellationToken)
    {
        var achievement = await _context.Achievements.FirstOrDefaultAsync(a => a.AchievementId == request.AchievementId, cancellationToken);
        if (achievement == null)
        {
            return Result<int>.Failure("Achievement not found");
        }

        achievement.Name = request.Name;
        achievement.Description = request.Description;
        achievement.IconUrl = request.IconUrl;
        achievement.ConditionType = request.ConditionType;
        achievement.ConditionValue = request.ConditionValue;

        await _context.SaveChangesAsync(cancellationToken);
        
        return Result<int>.Success(achievement.AchievementId);
    }
}
