using System;
using System.Threading.Tasks;
using AcadPrep.Application.Features.Performance.Queries.GetPerformanceAnalysis;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcadPrep.WebUI.Pages.Performance;

[Authorize]
public class PerformanceAnalysisModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public PerformanceAnalysisModel(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    public PerformanceAnalysisDto AnalysisData { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        if (!int.TryParse(_currentUserService.UserId, out int parsedUserId))
        {
            return Unauthorized();
        }

        AnalysisData = (await _mediator.Send(new GetPerformanceAnalysisQuery(parsedUserId))).Data!;

        return Page();
    }
}
