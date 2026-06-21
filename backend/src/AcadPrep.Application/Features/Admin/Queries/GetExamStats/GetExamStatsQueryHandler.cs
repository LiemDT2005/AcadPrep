using AcadPrep.Application.Common.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcadPrep.Application.Features.Admin.DTOs;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Admin.Queries.GetExamStats;

public class GetExamStatsQueryHandler : IRequestHandler<GetExamStatsQuery, Result<List<ExamStatsDto>>>
{
    private readonly IAppDbContext _context;

    public GetExamStatsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<ExamStatsDto>>> Handle(GetExamStatsQuery request, CancellationToken cancellationToken)
    {
        var groupedStats = await _context.ExamAttempts
            .Include(ea => ea.Exam)
            .GroupBy(ea => new { ea.ExamId, ea.Exam.Title })
            .Select(g => new ExamStatsDto
            {
                ExamId = g.Key.ExamId,
                ExamName = g.Key.Title,
                TotalAttempts = g.Count(),
                AverageScore = g.Average(ea => (double)ea.TotalScore),
                HighestScore = g.Max(ea => ea.TotalScore),
                LowestScore = g.Min(ea => ea.TotalScore)
            })
            .ToListAsync(cancellationToken);

        return groupedStats.OrderByDescending(s => s.TotalAttempts).ToList();
    }
}

