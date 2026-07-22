using AcadPrep.Application.Features.Billing.Commands.CreatePaymentOrder;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcadPrep.WebUI.Pages.Payment;

[Authorize]
public class CheckoutModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public CheckoutModel(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    public IActionResult OnGet() => RedirectToPage("/Pricing");

    public async Task<IActionResult> OnPostAsync(int planId)
    {
        if (!int.TryParse(_currentUser.UserId, out var userId))
        {
            return Challenge();
        }

        var returnUrl = Url.Page("/Payment/Return", pageHandler: null, values: null, protocol: Request.Scheme)
                        ?? $"{Request.Scheme}://{Request.Host}/Payment/Return";

        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        if (Request.Headers.TryGetValue("X-Forwarded-For", out var forwarded))
        {
            var first = forwarded.ToString().Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault();
            if (!string.IsNullOrEmpty(first))
            {
                clientIp = first;
            }
        }

        var result = await _mediator.Send(new CreatePaymentOrderCommand(userId, planId, clientIp, returnUrl));
        if (!result.IsSuccess || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Error ?? "Could not create the payment order.";
            return RedirectToPage("/Pricing");
        }

        return Redirect(result.Data.PaymentUrl);
    }
}
