using System.Threading.Tasks;
using AcadPrep.Application.Features.Admin.Dashboard.Queries.GetUserStats;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Microsoft.AspNetCore.Authorization;
using Domain.Enums;

namespace AcadPrep.WebUI.Pages.Admin;

[Authorize(Roles = nameof(UserRole.Admin))]
public class DashboardModel : PageModel
{
    private readonly IMediator _mediator;

    public DashboardModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public UserStatsDto Stats { get; set; } = new();
    public double ActiveUserRate => CalculateRate(Stats.ActiveUsers, Stats.TotalUsers);
    public double NewUserRate => CalculateRate(Stats.NewRegistrations, Stats.TotalUsers);

    public async Task<IActionResult> OnGetAsync()
    {
        var result = await _mediator.Send(new GetUserStatsQuery());
        if (result.IsSuccess && result.Data != null)
        {
            Stats = result.Data;
        }
        return Page();
    }

    private static double CalculateRate(int value, int total)
    {
        if (total <= 0)
        {
            return 0;
        }

        return Math.Clamp(value * 100d / total, 0, 100);
    }
}


