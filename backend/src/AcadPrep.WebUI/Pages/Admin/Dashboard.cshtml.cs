using System.Threading.Tasks;
using AcadPrep.Application.Features.Admin.DTOs;
using AcadPrep.Application.Features.Admin.Queries.GetUserStats;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcadPrep.WebUI.Pages.Admin;

public class DashboardModel : PageModel
{
    private readonly IMediator _mediator;

    public DashboardModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public UserStatsDto Stats { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var result = await _mediator.Send(new GetUserStatsQuery());
        if (result.IsSuccess && result.Data != null)
        {
            Stats = result.Data;
        }
        return Page();
    }
}


