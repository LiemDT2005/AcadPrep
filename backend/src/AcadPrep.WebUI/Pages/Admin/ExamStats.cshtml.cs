using System.Collections.Generic;
using System.Threading.Tasks;
using AcadPrep.Application.Features.Admin.DTOs;
using AcadPrep.Application.Features.Admin.Queries.GetExamStats;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcadPrep.WebUI.Pages.Admin;

public class ExamStatsModel : PageModel
{
    private readonly IMediator _mediator;

    public ExamStatsModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public List<ExamStatsDto> Stats { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        Stats = (await _mediator.Send(new GetExamStatsQuery())).Data!;
        return Page();
    }
}


