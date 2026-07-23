using AcadPrep.Application.Features.Billing.Commands.GrantProSubscription;
using AcadPrep.Application.Features.Billing.Queries.GetAdminOrders;
using AcadPrep.Application.Features.Billing.Queries.GetPricingPlans;
using Application.Common.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AcadPrep.WebUI.Pages.Admin.Billing;

[Authorize(Roles = nameof(UserRole.Admin))]
public class IndexModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public IndexModel(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    public AcadPrep.Application.Common.Models.PaginatedList<AdminOrderDto>? Orders { get; set; }
    public List<SelectListItem> PlanOptions { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty]
    public int GrantUserId { get; set; }

    [BindProperty]
    public int GrantPlanId { get; set; }

    [BindProperty]
    public string? GrantNote { get; set; }

    public async Task OnGetAsync()
    {
        await LoadAsync();
    }

    public async Task<IActionResult> OnPostGrantAsync()
    {
        if (!int.TryParse(_currentUser.UserId, out var adminId))
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new GrantProSubscriptionCommand(
            GrantUserId, GrantPlanId, GrantNote, adminId));

        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] =
            result.IsSuccess
                ? $"Pro granted until {result.Data!.ExpiresAt:dd/MM/yyyy HH:mm} UTC."
                : result.Error;

        return RedirectToPage(new { Status, Search, PageNumber });
    }

    private async Task LoadAsync()
    {
        var orders = await _mediator.Send(new GetAdminOrdersQuery(PageNumber, 20, Status, Search));
        if (orders.IsSuccess)
        {
            Orders = orders.Data;
        }

        var plans = await _mediator.Send(new GetPricingPlansQuery(null));
        if (plans.IsSuccess && plans.Data is not null)
        {
            PlanOptions = plans.Data.Plans
                .Select(p => new SelectListItem($"{p.Name} ({p.DurationDays} days)", p.Id.ToString()))
                .ToList();
        }
    }
}
