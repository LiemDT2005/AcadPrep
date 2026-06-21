using AcadPrep.Application.Features.Performance.DTOs;
using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.Performance.Queries.GetDashboardData;

public record GetDashboardDataQuery(int UserId) : IRequest<Result<DashboardDataDto>>;
