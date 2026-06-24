using System.Threading.Tasks;
using AcadPrep.Application.Features.Admin.Commands.EditAchievement;
using AcadPrep.Application.Features.Admin.Queries.GetAchievementById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcadPrep.WebUI.Pages.Admin.Achievements;

public class EditModel : PageModel
{
    private readonly IMediator _mediator;

    public EditModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    [BindProperty]
    public EditAchievementCommand Command { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var result = await _mediator.Send(new GetAchievementByIdQuery(id));
        if (!result.IsSuccess)
        {
            return NotFound();
        }

        Command = new EditAchievementCommand
        {
            AchievementId = result.Data.AchievementId,
            Name = result.Data.Name,
            Description = result.Data.Description,
            IconUrl = result.Data.IconUrl,
            ConditionType = result.Data.ConditionType,
            ConditionValue = result.Data.ConditionValue
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _mediator.Send(Command);
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "Achievement badge updated successfully!";
            return RedirectToPage("./Index");
        }

        ModelState.AddModelError(string.Empty, result.Error);
        return Page();
    }
}
