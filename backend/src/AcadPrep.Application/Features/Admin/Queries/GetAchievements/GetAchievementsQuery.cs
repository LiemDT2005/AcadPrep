using System.Collections.Generic;
using AcadPrep.Application.Common.Models;
using Domain.Entities;
using MediatR;

using AcadPrep.Application.Features.Admin.DTOs;

namespace AcadPrep.Application.Features.Admin.Queries.GetAchievements;

public class GetAchievementsQuery : IRequest<Result<PaginatedList<AchievementAdminDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
