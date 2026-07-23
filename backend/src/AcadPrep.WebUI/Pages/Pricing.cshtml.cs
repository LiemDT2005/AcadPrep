using AcadPrep.Application.Features.Billing.Queries.GetPricingPlans;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcadPrep.WebUI.Pages;

public class PricingModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public PricingModel(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    public PricingPageDto? PageData { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync()
    {
        if (TempData["ErrorMessage"] is string tempError)
        {
            ErrorMessage = tempError;
        }

        int? userId = null;
        if (int.TryParse(_currentUser.UserId, out var id))
        {
            userId = id;
        }

        var result = await _mediator.Send(new GetPricingPlansQuery(userId));
        if (result.IsSuccess && result.Data is not null)
        {
            PageData = result.Data;
        }
        else
        {
            ErrorMessage = result.Error ?? "Could not load pricing.";
        }
    }
}
