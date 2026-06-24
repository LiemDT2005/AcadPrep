using Application.Common.Interfaces;
using Application.Features.Exam.Queries.GetExamDetail;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace AcadPrep.WebUI.Pages.Exams
{
    public class DetailModel : PageModel
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;

        public DetailModel(IMediator mediator, ICurrentUserService currentUserService)
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
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
            else
            {
                ErrorMessage = result.Error ?? "An error occurred while loading the exam detail.";
                TempData["ErrorMessage"] = ErrorMessage;
                return RedirectToPage("/Exams/Index");
            }
        }
    }
}
