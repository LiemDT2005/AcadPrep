using AcadPrep.Application.Common.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcadPrep.Application.Features.Admin.DTOs;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Admin.Queries.GetProgressReport;

public class GetProgressReportQueryHandler : IRequestHandler<GetProgressReportQuery, Result<ComprehensiveReportDto>>
{
    private readonly IAppDbContext _context;

    public GetProgressReportQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ComprehensiveReportDto>> Handle(GetProgressReportQuery request, CancellationToken cancellationToken)
    {
        // Group ExamAttempts by date
        var examAttemptsGrouped = await _context.ExamAttempts
            .Where(ea => ea.StartedAt >= request.StartDate && ea.StartedAt <= request.EndDate)
            .GroupBy(ea => ea.StartedAt.Date)
            .Select(g => new
            {
                Date = g.Key,
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);

        var dataPoints = examAttemptsGrouped.Select(g => new ProgressDataPoint
        {
            Date = DateOnly.FromDateTime(g.Date),
            CompletedExamsCount = g.Count
        }).OrderBy(p => p.Date).ToList();

        return new ComprehensiveReportDto
        {
            DataPoints = dataPoints
        };
    }
}

