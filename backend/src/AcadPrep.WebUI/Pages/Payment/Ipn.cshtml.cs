using AcadPrep.Application.Features.Billing.Commands.ProcessVNPayIpn;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcadPrep.WebUI.Pages.Payment;

/// <summary>
/// IPN endpoint — VNPay gọi server-to-server. Trả JSON { RspCode, Message }.
/// Phải public HTTPS trên production và khai báo trên merchant portal.
/// </summary>
[AllowAnonymous]
[IgnoreAntiforgeryToken]
public class IpnModel : PageModel
{
    private readonly IMediator _mediator;

    public IpnModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        return await ProcessAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        return await ProcessAsync();
    }

    private async Task<IActionResult> ProcessAsync()
    {
        var query = Request.Query
            .SelectMany(kv => kv.Value.Select(v => new KeyValuePair<string, string>(kv.Key, v ?? string.Empty)))
            .ToList();

        // Một số cấu hình VNPay gửi form body
        if (Request.HasFormContentType)
        {
            foreach (var kv in Request.Form)
            {
                foreach (var v in kv.Value)
                {
                    query.Add(new KeyValuePair<string, string>(kv.Key, v ?? string.Empty));
                }
            }
        }

        var result = await _mediator.Send(new ProcessVNPayIpnCommand(query, IsIpn: true));
        var data = result.Data;
        return new JsonResult(new
        {
            RspCode = data?.RspCode ?? "99",
            Message = data?.Message ?? result.Error ?? "Unknown error"
        });
    }
}
