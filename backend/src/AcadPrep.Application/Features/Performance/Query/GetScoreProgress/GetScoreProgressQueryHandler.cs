using AcadPrep.Application.Common.Models;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Performance.Queries.GetScoreProgress;

public class GetScoreProgressQueryHandler : IRequestHandler<GetScoreProgressQuery, Result<ScoreProgressResultDto>>
{
    private readonly IAppDbContext _context;

    public GetScoreProgressQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ScoreProgressResultDto>> Handle(GetScoreProgressQuery request, CancellationToken cancellationToken)
    {
        var attempts = await _context.ExamAttempts
            .AsNoTracking()
            .Where(x => x.UserId == request.UserId && x.IsSubmitted)
            .OrderBy(x => x.StartedAt)
            .Select(x => new ScoreProgressDto
            {
                AttemptDate = x.StartedAt,
                Score = x.TotalScore
            })
            .ToListAsync(cancellationToken);

        if (attempts.Count < 2)
        {
            return new ScoreProgressResultDto
            {
                HasSufficientData = false,
                Message = "Cáº§n lÃ m tá»‘i thiá»ƒu 2 bÃ i thi Ä‘á»ƒ theo dÃµi tiáº¿n Ä‘á»™ cáº£i thiá»‡n Ä‘iá»ƒm sá»‘ (Take more tests to track progress)"
            };
        }

        return new ScoreProgressResultDto
        {
            HasSufficientData = true,
            Scores = attempts
        };
    }
}

