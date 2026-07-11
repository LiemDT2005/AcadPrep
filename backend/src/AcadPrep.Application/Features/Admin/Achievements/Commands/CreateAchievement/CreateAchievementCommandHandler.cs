using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using AcadPrep.Application.Common.Models;
using Domain.Entities;
using MediatR;

namespace AcadPrep.Application.Features.Admin.Achievements.Commands.CreateAchievement;

public class CreateAchievementCommandHandler : IRequestHandler<CreateAchievementCommand, Result<int>>
{
    private readonly IAppDbContext _context;

    public CreateAchievementCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(CreateAchievementCommand request, CancellationToken cancellationToken)
    {
        var achievement = new Achievement
        {
            Name = request.Name,
            Description = request.Description,
            IconUrl = request.IconUrl,
            ConditionType = request.ConditionType,
            ConditionValue = request.ConditionValue
        };

        _context.Achievements.Add(achievement);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(achievement.AchievementId);
    }
}
