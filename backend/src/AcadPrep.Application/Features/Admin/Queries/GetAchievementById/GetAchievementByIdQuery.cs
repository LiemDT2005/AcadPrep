using AcadPrep.Application.Common.Models;
using Domain.Entities;
using MediatR;

namespace AcadPrep.Application.Features.Admin.Queries.GetAchievementById;

public record GetAchievementByIdQuery(int AchievementId) : IRequest<Result<Achievement>>;
