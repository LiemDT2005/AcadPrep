using System.Collections.Generic;
using System.Threading.Tasks;
using AcadPrep.Application.Features.Admin.Achievements.Queries.GetAchievements;
using AcadPrep.Application.Features.Admin.Achievements.Commands.DeleteAchievement;
using AcadPrep.Application.Common.Models;
using AcadPrep.Application.Features.Admin.Achievements.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Microsoft.AspNetCore.Authorization;
using Domain.Enums;

namespace AcadPrep.WebUI.Pages.Admin.Achievements;

[Authorize(Roles = nameof(UserRole.Admin))]
public class IndexModel : PageModel
{
    private readonly IMediator _mediator;

    public IndexModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public PaginatedList<AchievementAdminDto>? Achievements { get; set; }

    public async Task<IActionResult> OnGetAsync(int pageNumber = 1)
    {
        var result = await _mediator.Send(new GetAchievementsQuery { PageNumber = pageNumber, PageSize = 10 });
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
