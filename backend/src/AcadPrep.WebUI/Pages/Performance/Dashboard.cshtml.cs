using System;
using System.Threading.Tasks;
using AcadPrep.Application.Features.Performance.DTOs;
using AcadPrep.Application.Features.Performance.Queries.GetStudyStreak;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcadPrep.WebUI.Pages.Performance;

public class DashboardModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public DashboardModel(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    public StudyStreakDto StreakData { get; set; } = new();
    public DashboardDataDto DashboardData { get; set; } = new();
    public AcadPrep.Application.Features.Performance.Queries.GetScoreProgress.ScoreProgressResultDto ScoreProgressData { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        if (string.IsNullOrEmpty(_currentUserService.UserId) || !int.TryParse(_currentUserService.UserId, out int parsedUserId))
        {
            parsedUserId = 2; // Fallback to Test User
        }

        // Reset streak if needed before querying
        await _mediator.Send(new AcadPrep.Application.Features.Performance.Commands.ResetStudyStreak.ResetStudyStreakCommand(parsedUserId));

        var streakResult = await _mediator.Send(new GetStudyStreakQuery(parsedUserId));
        if (streakResult.IsSuccess && streakResult.Data != null) StreakData = streakResult.Data;

        var dashResult = await _mediator.Send(new AcadPrep.Application.Features.Performance.Queries.GetDashboardData.GetDashboardDataQuery(parsedUserId));
        if (dashResult.IsSuccess && dashResult.Data != null) DashboardData = dashResult.Data;

        var scoreResult = await _mediator.Send(new AcadPrep.Application.Features.Performance.Queries.GetScoreProgress.GetScoreProgressQuery(parsedUserId));
        if (scoreResult.IsSuccess && scoreResult.Data != null) ScoreProgressData = scoreResult.Data;

        return Page();
    }
}


