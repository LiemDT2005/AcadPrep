using AcadPrep.Application.Common.Models;
using System.Collections.Generic;
using AcadPrep.Application.Features.Admin.DTOs;
using MediatR;

namespace AcadPrep.Application.Features.Admin.Queries.GetExamStats;

public record GetExamStatsQuery() : IRequest<Result<List<ExamStatsDto>>>;

