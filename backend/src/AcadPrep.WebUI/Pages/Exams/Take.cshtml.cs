using System.Threading.Tasks;
using AcadPrep.Application.Features.FullTest.Commands.SaveAnswer;
using AcadPrep.Application.Features.FullTest.Commands.SaveProgress;
using AcadPrep.Application.Features.FullTest.Commands.SubmitTest;
using AcadPrep.Application.Features.FullTest.Queries.GetTestSession;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcadPrep.WebUI.Pages.Exams;

[Authorize]
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

        if (!int.TryParse(_currentUserService.UserId, out int userId))
        {
            return Unauthorized();
        }

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
        if (!int.TryParse(_currentUserService.UserId, out int userId))
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new SaveAnswerCommand(
            request.AttemptId, userId, request.QuestionId, request.SelectedOption));

        return new JsonResult(new { success = result.IsSuccess, error = result.Error });
    }

    public async Task<IActionResult> OnPostSaveProgressAsync([FromBody] SaveProgressRequest request)
    {
        if (!int.TryParse(_currentUserService.UserId, out int userId))
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new SaveProgressCommand(
            request.AttemptId, userId, request.RemainingSeconds));

        return new JsonResult(new { success = result.IsSuccess, error = result.Error });
    }

    public async Task<IActionResult> OnPostSubmitAsync([FromBody] SubmitRequest request)
    {
        if (!int.TryParse(_currentUserService.UserId, out int userId))
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new SubmitTestCommand(
            request.AttemptId, userId, request.RemainingSeconds));

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
