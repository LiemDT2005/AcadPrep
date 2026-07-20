using System.Threading.Tasks;
using AcadPrep.Application.Features.FullTest.Queries.GetAttemptReview;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcadPrep.WebUI.Pages.Exams;

[Authorize]
public class ReviewModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public ReviewModel(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    [BindProperty(SupportsGet = true)]
    public int? AttemptId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? SessionId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string Filter { get; set; } = "all";

    public AttemptReviewDto Review { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        if (!int.TryParse(_currentUserService.UserId, out int userId))
        {
            return Unauthorized();
        }

        if (!AttemptId.HasValue && !SessionId.HasValue)
        {
            return RedirectToPage("/Exams/Index");
        }

        var result = await _mediator.Send(new GetAttemptReviewQuery(userId, AttemptId, SessionId));
        if (!result.IsSuccess || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Error ?? "Không tải được bài review.";
            return RedirectToPage("/Exams/Index");
        }

        Review = result.Data;
        return Page();
    }
}
