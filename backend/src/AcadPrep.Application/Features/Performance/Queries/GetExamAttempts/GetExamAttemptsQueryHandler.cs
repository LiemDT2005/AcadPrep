using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcadPrep.Application.Common.Models;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Performance.Queries.GetExamAttempts;

public class GetExamAttemptsQueryHandler : IRequestHandler<GetExamAttemptsQuery, Result<ExamAttemptsResultDto>>
{
    private readonly IAppDbContext _context;

    public GetExamAttemptsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ExamAttemptsResultDto>> Handle(GetExamAttemptsQuery request, CancellationToken cancellationToken)
    {
        var attempts = await _context.ExamAttempts
            .AsNoTracking()
            .Where(a => a.UserId == request.UserId && a.IsSubmitted)
            .OrderByDescending(a => a.CompletedAt ?? a.StartedAt)
            .Select(a => new ExamAttemptListItemDto
            {
                AttemptId = a.Id,
                ExamId = a.ExamId,
                ExamTitle = a.Exam.Title,
                ListeningScore = a.ListeningScore,
                ReadingScore = a.ReadingScore,
                TotalScore = a.TotalScore,
                StartedAt = a.StartedAt,
                CompletedAt = a.CompletedAt
            })
            .ToListAsync(cancellationToken);

        return new ExamAttemptsResultDto { Attempts = attempts };
    }
}
