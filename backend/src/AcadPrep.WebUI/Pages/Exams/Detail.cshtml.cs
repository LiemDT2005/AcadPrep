using Application.Common.Interfaces;
using Application.Features.Exam.Queries.GetExamDetail;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.WebUI.Pages.Exams
{
    [IgnoreAntiforgeryToken]
    public class DetailModel : PageModel
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAppDbContext _context;

        public DetailModel(IMediator mediator, ICurrentUserService currentUserService, IAppDbContext context)
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
            _context = context;
        }

        public GetExamDetailDto? ExamDetail { get; set; }
        public string? ErrorMessage { get; set; }
        public bool IsLoggedIn { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            IsLoggedIn = !string.IsNullOrEmpty(_currentUserService.UserId);
            int? parsedUserId = null;
            if (int.TryParse(_currentUserService.UserId, out var userId))
            {
                parsedUserId = userId;
            }

            var query = new GetExamDetailQuery(id, parsedUserId);
            var result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data is not null)
            {
                ExamDetail = result.Data;
                return Page();
            }

            ErrorMessage = result.Error ?? "An error occurred while loading the exam detail.";
            TempData["ErrorMessage"] = ErrorMessage;
            return RedirectToPage("/Exams/Index");
        }

        public async Task<IActionResult> OnPostStartPracticeAsync([FromBody] StartPracticeRequestModel request)
        {
            var userId = ResolveUserId();

            var command = new AcadPrep.Application.Features.Practice.Commands.StartPractice.StartPracticeCommand(
                request.ExamId,
                request.SelectedPartNumbers,
                request.SelectedTags,
                request.TimeLimitMinutes,
                userId
            );

            var result = await _mediator.Send(command);
            if (result.IsSuccess)
            {
                return new JsonResult(new { success = true, sessionId = result.Data });
            }

            return new JsonResult(new { success = false, error = result.Error });
        }

        public async Task<IActionResult> OnPostStartFullTestAsync([FromBody] StartFullTestRequestModel request)
        {
            var userId = ResolveUserId();

            if (!request.StartNewAttempt)
            {
                var inProgress = await _context.ExamAttempts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.ExamId == request.ExamId && a.UserId == userId && !a.IsSubmitted);

                if (inProgress is not null)
                {
                    return new JsonResult(new
                    {
                        success = false,
                        inProgressAttemptId = inProgress.Id,
                        remainingSeconds = inProgress.RemainingTime,
                        error = $"You have an unfinished test ({TimeSpan.FromSeconds(inProgress.RemainingTime):hh\\:mm\\:ss} remaining)."
                    });
                }
            }

            var command = new AcadPrep.Application.Features.FullTest.Commands.StartFullTest.StartFullTestCommand(
                request.ExamId,
                userId,
                request.StartNewAttempt);

            var result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data is not null)
            {
                return new JsonResult(new
                {
                    success = true,
                    attemptId = result.Data.AttemptId,
                    abandonedAttemptId = result.Data.AbandonedAttemptId
                });
            }

            return new JsonResult(new { success = false, error = result.Error });
        }

        private int ResolveUserId()
        {
            if (!string.IsNullOrEmpty(_currentUserService.UserId) && int.TryParse(_currentUserService.UserId, out int parsedUserId))
            {
                return parsedUserId;
            }

            return 2;
        }
    }

    public class StartPracticeRequestModel
    {
        public int ExamId { get; set; }
        public List<int> SelectedPartNumbers { get; set; } = new();
        public List<string> SelectedTags { get; set; } = new();
        public int? TimeLimitMinutes { get; set; }
    }

    public class StartFullTestRequestModel
    {
        public int ExamId { get; set; }
        public bool StartNewAttempt { get; set; }
    }
}
