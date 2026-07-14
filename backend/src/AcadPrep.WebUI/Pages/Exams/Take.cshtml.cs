using AcadPrep.Application.Features.FullTest.Commands.SaveAnswer;
using AcadPrep.Application.Features.FullTest.Commands.SaveProgress;
using AcadPrep.Application.Features.FullTest.Commands.SubmitTest;
using AcadPrep.Application.Features.FullTest.Queries.GetTestSession;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcadPrep.WebUI.Pages.Exams;

[IgnoreAntiforgeryToken]
public class TakeModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public TakeModel(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    [BindProperty(SupportsGet = true)]
    public int? AttemptId { get; set; }

    public TestSessionDto? Session { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (!AttemptId.HasValue)
        {
            return RedirectToPage("/Exams/Index");
        }

        var userId = ResolveUserId();
        var result = await _mediator.Send(new GetTestSessionQuery(AttemptId.Value, userId));

        if (!result.IsSuccess || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Error ?? "Unable to load test session.";
            return RedirectToPage("/Exams/Index");
        }

        Session = result.Data;
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAnswerAsync([FromBody] SaveAnswerRequest request)
    {
        var result = await _mediator.Send(new SaveAnswerCommand(
            request.AttemptId, ResolveUserId(), request.QuestionId, request.SelectedOption));

        return new JsonResult(new { success = result.IsSuccess, error = result.Error });
    }

    public async Task<IActionResult> OnPostSaveProgressAsync([FromBody] SaveProgressRequest request)
    {
        var result = await _mediator.Send(new SaveProgressCommand(
            request.AttemptId, ResolveUserId(), request.RemainingSeconds));

        return new JsonResult(new { success = result.IsSuccess, error = result.Error });
    }

    public async Task<IActionResult> OnPostSubmitAsync([FromBody] SubmitRequest request)
    {
        var result = await _mediator.Send(new SubmitTestCommand(
            request.AttemptId, ResolveUserId(), request.RemainingSeconds));

        if (result.IsSuccess && result.Data is not null)
        {
            return new JsonResult(new
            {
                success = true,
                attemptId = result.Data.AttemptId,
                listeningScore = result.Data.ListeningScore,
                readingScore = result.Data.ReadingScore,
                totalScore = result.Data.TotalScore
            });
        }

        return new JsonResult(new { success = false, error = result.Error });
    }

    private int ResolveUserId()
    {
        if (!string.IsNullOrEmpty(_currentUserService.UserId) && int.TryParse(_currentUserService.UserId, out int id))
        {
            return id;
        }

        return 2;
    }

    public class SaveAnswerRequest
    {
        public int AttemptId { get; set; }
        public int QuestionId { get; set; }
        public string? SelectedOption { get; set; }
    }

    public class SaveProgressRequest
    {
        public int AttemptId { get; set; }
        public int RemainingSeconds { get; set; }
    }

    public class SubmitRequest
    {
        public int AttemptId { get; set; }
        public int? RemainingSeconds { get; set; }
    }
}
