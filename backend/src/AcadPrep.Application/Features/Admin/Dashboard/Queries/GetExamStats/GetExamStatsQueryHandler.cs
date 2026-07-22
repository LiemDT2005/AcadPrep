using AcadPrep.Application.Common.Models;
using System.Collections.Generic;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace AcadPrep.Application.Features.Admin.Dashboard.Queries.GetExamStats;

public class GetExamStatsQueryHandler : IRequestHandler<GetExamStatsQuery, Result<PaginatedList<ExamStatsDto>>>
{
    private readonly IAppDbContext _context;

    public GetExamStatsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedList<ExamStatsDto>>> Handle(GetExamStatsQuery request, CancellationToken cancellationToken)
    {
        var groupedStatsQuery = _context.ExamAttempts
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
            .OrderByDescending(s => s.TotalAttempts);

        var paginatedResult = await PaginatedList<ExamStatsDto>.CreateAsync(
            groupedStatsQuery,
            request.PageNumber,
            request.PageSize
        );

        return Result<PaginatedList<ExamStatsDto>>.Success(paginatedResult);
    }
}

