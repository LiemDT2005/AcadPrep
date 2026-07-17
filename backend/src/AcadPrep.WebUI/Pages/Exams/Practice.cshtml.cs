using System.Collections.Generic;
using System.Threading.Tasks;
using AcadPrep.Application.Features.Practice.Commands.SubmitPractice;
using AcadPrep.Application.Features.Practice.Queries.GetPracticeSession;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.WebUI.Pages.Exams;

[Authorize]
[IgnoreAntiforgeryToken]
public class PracticeModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAppDbContext _context;

    public PracticeModel(IMediator mediator, ICurrentUserService currentUserService, IAppDbContext context)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _context = context;
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

        if (!int.TryParse(_currentUserService.UserId, out int userId))
        {
            return Unauthorized();
        }

        var existing = await _context.PracticeSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == SessionId.Value && s.UserId == userId);

        if (existing is null)
        {
            TempData["ErrorMessage"] = "Unable to load practice session.";
            return RedirectToPage("/Exams/Index");
        }

        if (existing.IsSubmitted)
        {
            return RedirectToPage("/Exams/Results", new { sessionId = existing.Id });
        }

        var result = await _mediator.Send(new GetPracticeSessionQuery(SessionId.Value, userId));

        if (!result.IsSuccess || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Error ?? "Unable to load practice session.";
            return RedirectToPage("/Exams/Index");
        }

        Session = result.Data;
        return Page();
    }

    public async Task<IActionResult> OnPostSubmitAsync([FromBody] SubmitPracticeRequest request)
    {
        if (!int.TryParse(_currentUserService.UserId, out int userId))
        {
            return Unauthorized();
        }

        var answers = new Dictionary<int, string>();
        if (request.Answers is not null)
        {
            foreach (var (key, value) in request.Answers)
            {
                if (int.TryParse(key, out var questionId) && !string.IsNullOrWhiteSpace(value))
                {
                    answers[questionId] = value;
                }
            }
        }

        var result = await _mediator.Send(new SubmitPracticeCommand(
            request.SessionId, userId, answers));

        if (result.IsSuccess && result.Data is not null)
        {
            return new JsonResult(new
            {
                success = true,
                sessionId = result.Data.SessionId,
                examId = result.Data.ExamId,
                correctCount = result.Data.CorrectCount,
                totalQuestions = result.Data.TotalQuestions,
                listeningCorrect = result.Data.ListeningCorrect,
                readingCorrect = result.Data.ReadingCorrect
            });
        }

        return new JsonResult(new { success = false, error = result.Error });
    }

    public class SubmitPracticeRequest
    {
        public int SessionId { get; set; }
        public Dictionary<string, string>? Answers { get; set; }
    }
}
