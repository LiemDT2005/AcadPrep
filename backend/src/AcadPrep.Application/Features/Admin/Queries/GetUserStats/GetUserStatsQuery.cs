using AcadPrep.Application.Features.Admin.DTOs;
using MediatR;

namespace AcadPrep.Application.Features.Admin.Queries.GetUserStats;

public record GetUserStatsQuery() : IRequest<Result<UserStatsDto>>;

