using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.Performance.Commands.CheckAndGrantAchievements;

public record CheckAndGrantAchievementsCommand(int UserId) : IRequest<Result<bool>>;
