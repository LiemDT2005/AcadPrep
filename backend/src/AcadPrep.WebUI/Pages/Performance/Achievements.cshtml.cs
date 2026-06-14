using System.Collections.Generic;
using System.Threading.Tasks;
using AcadPrep.Application.Features.Performance.DTOs;
using AcadPrep.Application.Features.Performance.Queries.GetAchievements;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcadPrep.WebUI.Pages.Performance;

public class AchievementsModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public AchievementsModel(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    public List<AchievementDto> Achievements { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        if (string.IsNullOrEmpty(_currentUserService.UserId) || !int.TryParse(_currentUserService.UserId, out int parsedUserId))
        {
            parsedUserId = 1; // Fallback
        }

        Achievements = await _mediator.Send(new GetAchievementsQuery(parsedUserId));

        return Page();
    }
}
