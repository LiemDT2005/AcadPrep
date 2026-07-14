using Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.WebUI.Pages.Exams;

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

    public int TotalScore { get; set; }
    public int ListeningScore { get; set; }
    public int ReadingScore { get; set; }
    public int ExamId { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (!AttemptId.HasValue)
        {
            return RedirectToPage("/Exams/Index");
        }

        var userId = ResolveUserId();
        var attempt = await _context.ExamAttempts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == AttemptId.Value && a.UserId == userId && a.IsSubmitted);

        if (attempt is null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy kết quả bài thi.";
            return RedirectToPage("/Exams/Index");
        }

        TotalScore = attempt.TotalScore;
        ListeningScore = attempt.ListeningScore;
        ReadingScore = attempt.ReadingScore;
        ExamId = attempt.ExamId;
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
