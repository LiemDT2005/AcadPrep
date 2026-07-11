using System.Collections.Generic;
using System.Threading.Tasks;
using AcadPrep.Application.Features.Admin.Exams.Commands.DeleteExamContent;
using AcadPrep.Application.Features.Admin.Exams.Queries.Common.DTOs;
using AcadPrep.Application.Features.Admin.Exams.Queries.GetExamDetail;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcadPrep.WebUI.Pages.Admin.Exams;

public class EditModel(ISender mediator) : PageModel
{
    public ExamDetailDto Exam { get; set; } = null!;
    
    [TempData]
    public string? ErrorMessage { get; set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var result = await mediator.Send(new GetExamDetailQuery { Id = id });

        if (!result.IsSuccess || result.Data == null)
        {
            return RedirectToPage("./Index");
        }

        Exam = result.Data;
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id, string contentType, int targetId)
    {
        var result = await mediator.Send(new DeleteExamContentCommand
        {
            ExamId = id,
            ContentType = contentType,
            TargetId = targetId
        });

        if (result.IsSuccess)
        {
            SuccessMessage = "Content deleted successfully.";
        }
        else
        {
            ErrorMessage = result.Error ?? "Failed to delete content.";
        }

        return RedirectToPage("./Edit", new { id });
    }
}
