using AcadPrep.Application.Features.Billing.Commands.ProcessVNPayIpn;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcadPrep.WebUI.Pages.Payment;

[AllowAnonymous]
public class ReturnModel : PageModel
{
    private readonly IMediator _mediator;

    public ReturnModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public bool Success { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public string? OrderCode { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    public async Task OnGetAsync()
    {
        var query = Request.Query
            .SelectMany(kv => kv.Value.Select(v => new KeyValuePair<string, string>(kv.Key, v ?? string.Empty)))
            .ToList();

        var result = await _mediator.Send(new ProcessVNPayIpnCommand(query, IsIpn: false));
        if (result.IsSuccess && result.Data is not null)
        {
            Success = result.Data.PaymentSucceeded;
            OrderCode = result.Data.OrderCode;
            ExpiresAt = result.Data.SubscriptionExpiresAt;
            Message = Success
                ? "Payment successful. Your Pro plan has been activated."
                : (result.Data.Message == "Confirm Success"
                    ? "The transaction failed or was cancelled."
                    : result.Data.Message);
        }
        else
        {
            Success = false;
            Message = result.Error ?? "Could not verify the transaction.";
        }
    }
}
