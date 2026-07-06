using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Features.Exams.Queries.Common.DTOs;
using Application.Features.Exams.Queries.GetAdminExamList;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcadPrep.WebUI.Pages.Admin.Exams;

public class IndexModel(ISender mediator) : PageModel
{
    public List<AdminExamDto> Exams { get; set; } = new();

    public async Task OnGetAsync()
    {
        var result = await mediator.Send(new GetAdminExamListQuery());
        if (result.IsSuccess && result.Data != null)
        {
            Exams = result.Data;
        }
    }
}
