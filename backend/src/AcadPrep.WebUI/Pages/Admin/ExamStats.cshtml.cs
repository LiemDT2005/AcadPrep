using System.Collections.Generic;
using System.Threading.Tasks;
using AcadPrep.Application.Common.Models;
using AcadPrep.Application.Features.Admin.Dashboard.Queries.GetExamStats;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Microsoft.AspNetCore.Authorization;
using Domain.Enums;

namespace AcadPrep.WebUI.Pages.Admin;

[Authorize(Roles = nameof(UserRole.Admin))]
public class ExamStatsModel : PageModel
{
    private readonly IMediator _mediator;

    public ExamStatsModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public PaginatedList<ExamStatsDto>? Stats { get; set; }

    public async Task<IActionResult> OnGetAsync(int pageNumber = 1)
    {
        Stats = (await _mediator.Send(new GetExamStatsQuery(pageNumber, 10))).Data;
        return Page();
    }
}


