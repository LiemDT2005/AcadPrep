using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.Admin.Achievements.Commands.DeleteAchievement;

public record DeleteAchievementCommand(int AchievementId) : IRequest<Result<bool>>;
