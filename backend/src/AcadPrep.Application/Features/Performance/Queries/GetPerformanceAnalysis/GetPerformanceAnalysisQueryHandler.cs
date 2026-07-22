using AcadPrep.Application.Common.Models;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Performance.Queries.GetPerformanceAnalysis;

public class GetPerformanceAnalysisQueryHandler : IRequestHandler<GetPerformanceAnalysisQuery, Result<PerformanceAnalysisDto>>
{
    private readonly IAppDbContext _context;

    public GetPerformanceAnalysisQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PerformanceAnalysisDto>> Handle(GetPerformanceAnalysisQuery request, CancellationToken cancellationToken)
    {
        var attempts = await _context.ExamAttempts
            .AsNoTracking()
            .Where(x => x.UserId == request.UserId && x.IsSubmitted)
            .ToListAsync(cancellationToken);

        if (!attempts.Any())
        {
            return new PerformanceAnalysisDto
            {
                HasData = false,
                Message = "ChÆ°a cÃ³ lá»‹ch sá»­ lÃ m bÃ i"
            };
        }

        var totalTests = attempts.Count;
        var avgListening = attempts.Average(x => x.ListeningScore);
        var avgReading = attempts.Average(x => x.ReadingScore);

        var result = new PerformanceAnalysisDto
        {
            HasData = true,
            TotalTestsTaken = totalTests,
            AverageListeningScore = avgListening,
            AverageReadingScore = avgReading
        };

        var userAnswers = await _context.AttemptAnswers
            .Include(a => a.Question)
            .Where(a => a.ExamAttempt.UserId == request.UserId && a.ExamAttempt.IsSubmitted)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        for (int p = 1; p <= 7; p++)
        {
            var partAnswers = userAnswers.Where(a => a.Question.Part == p).ToList();
            if (partAnswers.Any())
            {
                int correct = partAnswers.Count(a => a.IsCorrect);
                result.PartMastery[p] = (int)((double)correct / partAnswers.Count * 100);
            }
            else
            {
                result.PartMastery[p] = 0;
            }
        }

        return result;
    }
}

