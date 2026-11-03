using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AcadPrep.Application.Features.Practice.Commands.SubmitPractice;
using AcadPrep.Application.Features.Practice.Queries.GetPracticeSession;
using AcadPrep.Application.Features.Vocabulary.Commands.CreateVocabulary;
using AcadPrep.Application.Features.Vocabulary.Commands.SaveVocabulary;
using AcadPrep.Application.Features.Vocabulary.Queries.LookupVocabulary;
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
    private readonly ITranslationService _translationService;

    public PracticeModel(IMediator mediator, ICurrentUserService currentUserService, IAppDbContext context, ITranslationService translationService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _context = context;
        _translationService = translationService;
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

    public async Task<IActionResult> OnPostAddVocabularyAsync([FromBody] AddVocabularyRequest request)
    {
        if (!int.TryParse(_currentUserService.UserId, out int userId))
        {
            return Unauthorized();
        }

        var keyword = NormalizeSelectedWord(request.Keyword);
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return new JsonResult(new { success = false, error = "Please select a valid word." });
        }

        // Check if word already exists in the vocabulary database
        var lookupResult = await _mediator.Send(new LookupVocabularyQuery(keyword));

        if (lookupResult.IsSuccess && lookupResult.Data is not null)
        {
            // Word exists — save to user's notebook
            var saveResult = await _mediator.Send(new SaveVocabularyCommand(userId, lookupResult.Data.VocabularyId));
            return new JsonResult(new
            {
                success = true,
                status = saveResult.Data ? "saved" : "already_saved",
                word = lookupResult.Data.Word,
                meaning = lookupResult.Data.Meaning
            });
        }

        // Word not in DB — auto-translate via Google Translate then create
        var translatedMeaning = await _translationService.TranslateToVietnameseAsync(keyword);
        var meaning = !string.IsNullOrWhiteSpace(translatedMeaning)
            ? translatedMeaning
            : "(chưa có nghĩa — vui lòng cập nhật trong Notebook)";

        var createResult = await _mediator.Send(new CreateVocabularyCommand
        {
            UserId = userId,
            Word = keyword,
            Meaning = meaning
        });

        if (!createResult.IsSuccess)
        {
            return new JsonResult(new { success = false, error = createResult.Error ?? "Unable to save this word." });
        }

        return new JsonResult(new
        {
            success = true,
            status = "created",
            word = keyword,
            meaning
        });
    }

    public class SubmitPracticeRequest
    {
        public int SessionId { get; set; }
        public Dictionary<string, string>? Answers { get; set; }
    }

    public class AddVocabularyRequest
    {
        public string? Keyword { get; set; }
    }

    private static string NormalizeSelectedWord(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var parts = Regex.Matches(value.Trim(), "[A-Za-z]+(?:['’-][A-Za-z]+)?")
            .Select(match => match.Value)
            .Take(3)
            .ToList();

        return string.Join(" ", parts);
    }
}
