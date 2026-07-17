using System.Threading.Tasks;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.WebUI.Pages.Exams;

[Authorize]
public class ResultsModel : PageModel
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public ResultsModel(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    [BindProperty(SupportsGet = true)]
    public int? AttemptId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? SessionId { get; set; }

    public bool IsPractice { get; set; }
    public int TotalScore { get; set; }
    public int MaxScore { get; set; } = 990;
    public int ListeningScore { get; set; }
    public int ReadingScore { get; set; }
    public int ListeningMax { get; set; }
    public int ReadingMax { get; set; }
    public int ExamId { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (!int.TryParse(_currentUserService.UserId, out int userId))
        {
            return Unauthorized();
        }

        if (SessionId.HasValue)
        {
            return await LoadPracticeResultsAsync(SessionId.Value, userId);
        }

        if (AttemptId.HasValue)
        {
            return await LoadFullTestResultsAsync(AttemptId.Value, userId);
        }

        return RedirectToPage("/Exams/Index");
    }

    private async Task<IActionResult> LoadFullTestResultsAsync(int attemptId, int userId)
    {
        var attempt = await _context.ExamAttempts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == attemptId && a.UserId == userId && a.IsSubmitted);

        if (attempt is null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy kết quả bài thi.";
            return RedirectToPage("/Exams/Index");
        }

        IsPractice = false;
        TotalScore = attempt.TotalScore;
        MaxScore = 990;
        ListeningScore = attempt.ListeningScore;
        ReadingScore = attempt.ReadingScore;
        ListeningMax = 495;
        ReadingMax = 495;
        ExamId = attempt.ExamId;
        return Page();
    }

    private async Task<IActionResult> LoadPracticeResultsAsync(int sessionId, int userId)
    {
        var session = await _context.PracticeSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId && s.IsSubmitted);

        if (session is null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy kết quả luyện tập.";
            return RedirectToPage("/Exams/Index");
        }

        IsPractice = true;
        TotalScore = session.CorrectCount;
        MaxScore = session.TotalQuestions;
        ListeningScore = session.ListeningCorrect;
        ReadingScore = session.ReadingCorrect;
        ListeningMax = session.ListeningTotal;
        ReadingMax = session.ReadingTotal;
        ExamId = session.ExamId;
        return Page();
    }
}
