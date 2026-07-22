using System;
using System.Threading.Tasks;
using AcadPrep.Application.Features.Performance.Queries.GetScoreProgress;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcadPrep.WebUI.Pages.Performance;

[Authorize]
public class ScoreProgressModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public ScoreProgressModel(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    public ScoreProgressResultDto ProgressData { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        if (!int.TryParse(_currentUserService.UserId, out int parsedUserId))
        {
            return Unauthorized();
        }

        ProgressData = (await _mediator.Send(new GetScoreProgressQuery(parsedUserId))).Data!;

        return Page();
    }
}
