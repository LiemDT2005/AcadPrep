using System.Threading.Tasks;
using AcadPrep.Application.Features.Admin.Exams.Queries.Common.DTOs;
using AcadPrep.Application.Features.Admin.Exams.Queries.GetExamDetail;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcadPrep.WebUI.Pages.Admin.Exams;

public class DetailModel(ISender mediator) : PageModel
{
    public ExamDetailDto Exam { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var result = await mediator.Send(new GetExamDetailQuery { Id = id });
        
        if (!result.IsSuccess || result.Data == null)
        {
            return RedirectToPage("/Admin/Exams/Index");
        }

        Exam = result.Data;
        return Page();
    }
}
