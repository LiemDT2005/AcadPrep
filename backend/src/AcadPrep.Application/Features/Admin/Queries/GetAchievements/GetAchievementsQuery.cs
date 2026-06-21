using System.Collections.Generic;
using AcadPrep.Application.Common.Models;
using Domain.Entities;
using MediatR;

namespace AcadPrep.Application.Features.Admin.Queries.GetAchievements;

public class GetAchievementsQuery : IRequest<Result<List<Achievement>>>
{
}
