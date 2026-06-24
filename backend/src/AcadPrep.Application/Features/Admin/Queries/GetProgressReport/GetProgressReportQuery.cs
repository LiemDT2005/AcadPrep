using AcadPrep.Application.Common.Models;
using System;
using AcadPrep.Application.Features.Admin.DTOs;
using MediatR;

namespace AcadPrep.Application.Features.Admin.Queries.GetProgressReport;

public record GetProgressReportQuery(DateTime StartDate, DateTime EndDate) : IRequest<Result<ComprehensiveReportDto>>;

