using AcadPrep.Application.Common.Models;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.FullTest.Commands.SaveProgress;

public class SaveProgressCommandHandler : IRequestHandler<SaveProgressCommand, Result>
{
    private readonly IAppDbContext _context;

    public SaveProgressCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(SaveProgressCommand request, CancellationToken cancellationToken)
    {
        var attempt = await _context.ExamAttempts
            .FirstOrDefaultAsync(a => a.Id == request.AttemptId && a.UserId == request.UserId && !a.IsSubmitted, cancellationToken);

        if (attempt is null)
        {
            return Result.Failure("Phiên thi không hợp lệ.");
        }

        attempt.RemainingTime = Math.Max(0, request.RemainingSeconds);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
