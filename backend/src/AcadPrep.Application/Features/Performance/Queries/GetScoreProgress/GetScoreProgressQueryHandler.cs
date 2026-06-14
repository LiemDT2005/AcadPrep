using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Performance.Queries.GetScoreProgress;

public class GetScoreProgressQueryHandler : IRequestHandler<GetScoreProgressQuery, ScoreProgressResultDto>
{
    private readonly IAppDbContext _context;

    public GetScoreProgressQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<ScoreProgressResultDto> Handle(GetScoreProgressQuery request, CancellationToken cancellationToken)
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
                Message = "Cần làm tối thiểu 2 bài thi để theo dõi tiến độ cải thiện điểm số (Take more tests to track progress)"
            };
        }

        return new ScoreProgressResultDto
        {
            HasSufficientData = true,
            Scores = attempts
        };
    }
}
