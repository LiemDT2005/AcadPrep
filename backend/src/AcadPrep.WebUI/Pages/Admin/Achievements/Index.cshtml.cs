using System.Collections.Generic;
using System.Threading.Tasks;
using AcadPrep.Application.Features.Admin.Queries.GetAchievements;
using AcadPrep.Application.Features.Admin.Commands.DeleteAchievement;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcadPrep.WebUI.Pages.Admin.Achievements;

public class IndexModel : PageModel
{
    private readonly IMediator _mediator;

    public IndexModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public List<Achievement> Achievements { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var result = await _mediator.Send(new GetAchievementsQuery());
        if (result.IsSuccess && result.Data != null)
        {
            Achievements = result.Data;
        }
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var result = await _mediator.Send(new DeleteAchievementCommand(id));
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "Achievement badge deleted successfully!";
        }
        else
        {
            TempData["ErrorMessage"] = result.Error;
        }
        return RedirectToPage("./Index");
    }
}
