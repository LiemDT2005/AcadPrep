using AcadPrep.Application.Features.Practice.Queries.GetPracticeSession;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcadPrep.WebUI.Pages.Exams;

public class PracticeModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public PracticeModel(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    [BindProperty(SupportsGet = true)]
    public int? SessionId { get; set; }

    public PracticeSessionDto? Session { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (!SessionId.HasValue)
        {
            return RedirectToPage("/Exams/Index");
        }

        var userId = ResolveUserId();
        var result = await _mediator.Send(new GetPracticeSessionQuery(SessionId.Value, userId));

        if (!result.IsSuccess || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Error ?? "Unable to load practice session.";
            return RedirectToPage("/Exams/Index");
        }

        Session = result.Data;
        return Page();
    }

    private int ResolveUserId()
    {
        if (!string.IsNullOrEmpty(_currentUserService.UserId) && int.TryParse(_currentUserService.UserId, out int id))
        {
            return id;
        }

        return 2;
    }
}
