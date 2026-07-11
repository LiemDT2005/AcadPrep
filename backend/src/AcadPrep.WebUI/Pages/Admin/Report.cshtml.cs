using System;
using System.Threading.Tasks;
using AcadPrep.Application.Features.Admin.Dashboard.Queries.GetProgressReport;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcadPrep.WebUI.Pages.Admin;

public class ReportModel : PageModel
{
    private readonly IMediator _mediator;

    public ReportModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    [BindProperty(SupportsGet = true)]
    public DateTime StartDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime EndDate { get; set; }

    public ComprehensiveReportDto Report { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        if (StartDate == default)
            StartDate = DateTime.UtcNow.AddDays(-30);
            
        if (EndDate == default)
            EndDate = DateTime.UtcNow;

        Report = (await _mediator.Send(new GetProgressReportQuery(StartDate, EndDate))).Data!;
        
        return Page();
    }
}


