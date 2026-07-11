using AcadPrep.Application.Common.Models;
using Domain.Entities;
using MediatR;

using AcadPrep.Application.Features.Admin.Achievements.DTOs;

namespace AcadPrep.Application.Features.Admin.Achievements.Queries.GetAchievementById;

public record GetAchievementByIdQuery(int AchievementId) : IRequest<Result<AchievementAdminDto>>;
