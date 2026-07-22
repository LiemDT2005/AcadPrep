using AcadPrep.Application.Common.Models;
using System.Collections.Generic;
using MediatR;
namespace AcadPrep.Application.Features.Admin.Dashboard.Queries.GetExamStats;

public record GetExamStatsQuery(int PageNumber = 1, int PageSize = 10) : IRequest<Result<PaginatedList<ExamStatsDto>>>;

