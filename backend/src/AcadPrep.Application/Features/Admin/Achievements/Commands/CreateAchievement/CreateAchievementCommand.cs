using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.Admin.Achievements.Commands.CreateAchievement;

public class CreateAchievementCommand : IRequest<Result<int>>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public string ConditionType { get; set; } = string.Empty;
    public int ConditionValue { get; set; }
}
