using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.Performance.Queries.GetPerformanceAnalysis;

public record GetPerformanceAnalysisQuery(int UserId) : IRequest<Result<PerformanceAnalysisDto>>;

