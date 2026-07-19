using AcadPrep.Application.Common.Models;
using AcadPrep.Application.Features.Admin.Accounts.DTOs;
using Application.Common.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Admin.Accounts.Queries.GetAccountDetail;

public class GetAccountDetailQueryHandler : IRequestHandler<GetAccountDetailQuery, Result<AccountDetailDto>>
{
    private readonly IAppDbContext _context;

    public GetAccountDetailQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<AccountDetailDto>> Handle(
        GetAccountDetailQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
            return Result<AccountDetailDto>.Failure("Account not found.");

        var masterAdminId = await _context.Users
            .Where(u => u.Role.RoleName == nameof(UserRole.Admin))
            .OrderBy(u => u.Id)
            .Select(u => u.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var examsTaken = await _context.ExamAttempts
            .CountAsync(ea => ea.UserId == user.Id && ea.IsSubmitted, cancellationToken);

        var avgScore = examsTaken > 0
            ? await _context.ExamAttempts
                .Where(ea => ea.UserId == user.Id && ea.IsSubmitted)
                .AverageAsync(ea => ea.TotalScore, cancellationToken)
            : 0;

        var streak = await _context.StudyStreaks
            .Where(s => s.UserId == user.Id)
            .Select(s => s.CurrentStreak)
            .FirstOrDefaultAsync(cancellationToken);

        return new AccountDetailDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            AvatarUrl = user.AvatarUrl,
            RoleName = user.Role.RoleName,
            RoleId = user.RoleId,
            Status = user.Status.ToString(),
            CreatedAt = user.CreatedAt,
            IsMasterAdmin = user.Id == masterAdminId,
            GoogleId = user.GoogleId,
            ExamsTaken = examsTaken,
            AverageScore = avgScore,
            CurrentStreak = streak
        };
    }
}
