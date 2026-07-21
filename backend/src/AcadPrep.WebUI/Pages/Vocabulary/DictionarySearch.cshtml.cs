using System.Linq;
using System.Threading.Tasks;
using AcadPrep.Application.Features.Vocabulary.Commands.SaveVocabulary;
using AcadPrep.Application.Features.Vocabulary.Queries.GetSavedVocabularies;
using AcadPrep.Application.Features.Vocabulary.Queries.LookupVocabulary;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcadPrep.WebUI.Pages.Vocabulary;

[Authorize]
public class DictionarySearchModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public DictionarySearchModel(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    [BindProperty(SupportsGet = true)]
    public string? Keyword { get; set; }

    public bool HasSearched { get; set; }

    public VocabularyDetailDto? Vocabulary { get; set; }

    public bool IsSavedInNotebook { get; set; }

    public string? SaveMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        SaveMessage = TempData["SaveMessage"] as string;

        if (string.IsNullOrWhiteSpace(Keyword))
        {
            return Page();
        }

        HasSearched = true;
        Vocabulary = (await _mediator.Send(new LookupVocabularyQuery(Keyword))).Data;

        if (Vocabulary is not null && int.TryParse(_currentUserService.UserId, out int userId))
        {
            var saved = (await _mediator.Send(new GetSavedVocabulariesQuery(userId, 1, 100, Vocabulary.Word))).Data;
            IsSavedInNotebook = saved?.Items.Any(x => x.VocabularyId == Vocabulary.VocabularyId) ?? false;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync(int vocabularyId)
    {
        if (!int.TryParse(_currentUserService.UserId, out int userId))
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new SaveVocabularyCommand(userId, vocabularyId));
        TempData["SaveMessage"] = result.Data ? "saved" : "already_saved";

        return RedirectToPage(new { Keyword });
    }
}
