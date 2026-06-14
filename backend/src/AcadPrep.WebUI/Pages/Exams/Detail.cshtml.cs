using System.Threading.Tasks;
using Application.Features.Exams.Queries.Common.DTOs;
using Application.Features.Exams.Queries.GetExamDetail;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcadPrep.WebUI.Pages.Exams;

public class DetailModel(ISender mediator) : PageModel
{
    public ExamDetailDto Exam { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var result = await mediator.Send(new GetExamDetailQuery { Id = id });
        
        if (!result.IsSuccess || result.Data == null)
        {
            return RedirectToPage("/Exams/Index");
        }

        Exam = result.Data;
        return Page();
    }
}
