using AcadPrep.Application.Features.Performance.DTOs;
using AcadPrep.Application.Common.Models;
using MediatR;
using System.Collections.Generic;

namespace AcadPrep.Application.Features.Performance.Queries.GetLeaderboard;

public record GetLeaderboardQuery(int UserId, string SortBy) : IRequest<Result<LeaderboardResultDto>>;

public class LeaderboardResultDto
{
    public List<LeaderboardEntryDto> Entries { get; set; } = new();
    public LeaderboardEntryDto CurrentUserEntry { get; set; } = new();
}
