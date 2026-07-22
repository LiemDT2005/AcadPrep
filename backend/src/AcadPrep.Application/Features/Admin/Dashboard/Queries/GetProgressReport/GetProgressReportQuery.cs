using AcadPrep.Application.Common.Models;
using System;
using MediatR;
namespace AcadPrep.Application.Features.Admin.Dashboard.Queries.GetProgressReport;

public record GetProgressReportQuery(DateTime StartDate, DateTime EndDate) : IRequest<Result<ComprehensiveReportDto>>;

