using System.Threading.Tasks;
using AcadPrep.Application.Features.Admin.Achievements.Commands.CreateAchievement;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Microsoft.AspNetCore.Authorization;
using Domain.Enums;

namespace AcadPrep.WebUI.Pages.Admin.Achievements;

[Authorize(Roles = nameof(UserRole.Admin))]
public class CreateModel : PageModel
{
    private readonly IMediator _mediator;

    public CreateModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    [BindProperty]
    public CreateAchievementCommand Command { get; set; } = new();

    public void OnGet()
    {
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
            return RedirectToPage("./Index");
        }

        ModelState.AddModelError(string.Empty, "Failed to create achievement.");
        return Page();
    }
}
