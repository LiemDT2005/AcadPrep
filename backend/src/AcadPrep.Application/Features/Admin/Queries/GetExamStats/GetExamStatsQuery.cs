using AcadPrep.Application.Common.Models;
using System.Collections.Generic;
using AcadPrep.Application.Features.Admin.DTOs;
using MediatR;

namespace AcadPrep.Application.Features.Admin.Queries.GetExamStats;

public record GetExamStatsQuery(int PageNumber = 1, int PageSize = 10) : IRequest<Result<PaginatedList<ExamStatsDto>>>;

