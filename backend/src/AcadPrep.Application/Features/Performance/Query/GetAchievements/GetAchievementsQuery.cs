using AcadPrep.Application.Common.Models;
using System.Collections.Generic;
using AcadPrep.Application.Features.Performance.DTOs;
using MediatR;

namespace AcadPrep.Application.Features.Performance.Queries.GetAchievements;

public record GetAchievementsQuery(int UserId) : IRequest<Result<List<AchievementDto>>>;

