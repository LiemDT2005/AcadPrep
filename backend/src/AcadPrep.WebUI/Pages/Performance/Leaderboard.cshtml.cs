using System;
using System.Threading.Tasks;
using AcadPrep.Application.Features.Performance.Queries.GetLeaderboard;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcadPrep.WebUI.Pages.Performance;

[AllowAnonymous]
public class LeaderboardModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public LeaderboardModel(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    public LeaderboardResultDto LeaderboardData { get; set; } = new();
    
    [BindProperty(SupportsGet = true)]
    public string SortBy { get; set; } = "Score";

    public async Task<IActionResult> OnGetAsync()
    {
        // Leaderboard is public (UC-4), but if user is logged in, use their ID for highlighting
        int parsedUserId = 0;
        if (!string.IsNullOrEmpty(_currentUserService.UserId))
        {
            int.TryParse(_currentUserService.UserId, out parsedUserId);
        }

        // Default SortBy to "Score" if invalid value is provided
        if (string.IsNullOrEmpty(SortBy) || 
            (!string.Equals(SortBy, "Score", StringComparison.OrdinalIgnoreCase) && 
             !string.Equals(SortBy, "Streak", StringComparison.OrdinalIgnoreCase)))
        {
            SortBy = "Score";
        }

        var result = await _mediator.Send(new GetLeaderboardQuery(parsedUserId, SortBy));
        if (result.IsSuccess && result.Data != null)
        {
            LeaderboardData = result.Data;
        }

        return Page();
    }
}
