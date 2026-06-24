using System.Threading.Tasks;
using AcadPrep.Application.Features.Vocabulary.Queries.LookupVocabulary;
using AcadPrep.Application.Features.Vocabulary.Queries.GetSavedVocabularies;
using AcadPrep.Application.Features.Vocabulary.Commands.SaveVocabulary;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcadPrep.WebUI.Pages.Vocabulary;

public class DictionarySearchModel : PageModel
{
    private readonly IMediator _mediator;

    public DictionarySearchModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    [BindProperty(SupportsGet = true)]
    public string? Keyword { get; set; }

    public bool HasSearched { get; set; } = false;

    public VocabularyDetailDto? Vocabulary { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (!string.IsNullOrWhiteSpace(Keyword))
        {
            HasSearched = true;
            Vocabulary = (await _mediator.Send(new LookupVocabularyQuery(Keyword))).Data!;
        }

        return Page();
    }
}


