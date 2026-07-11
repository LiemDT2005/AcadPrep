using AcadPrep.Application.Common.Models;
using MediatR;
namespace AcadPrep.Application.Features.Admin.Dashboard.Queries.GetUserStats;

public record GetUserStatsQuery() : IRequest<Result<UserStatsDto>>;

