using System.Threading.Tasks;
using AcadPrep.Application.Features.Performance.Queries.GetStudyHistory;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcadPrep.WebUI.Pages.Performance;

[Authorize]
public class StudyHistoryModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public StudyHistoryModel(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    public StudyHistoryResultDto History { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        if (!int.TryParse(_currentUserService.UserId, out int userId))
        {
            return Unauthorized();
        }

        History = (await _mediator.Send(new GetStudyHistoryQuery(userId))).Data!;
        return Page();
    }
}
