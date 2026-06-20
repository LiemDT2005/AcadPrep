using AcadPrep.Application.Features.Admin.DTOs;
using MediatR;

namespace AcadPrep.Application.Features.Admin.Queries.GetUserStats;

// TODO: Create Response wrapper if needed or use DTO directly.
public record GetUserStatsQuery() : IRequest<UserStatsDto>;
