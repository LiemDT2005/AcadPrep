using System.Threading.Tasks;
using AcadPrep.Application.Features.Vocabulary.Queries.GetVocabPassage;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcadPrep.WebUI.Pages.Vocabulary;

[Authorize]
public class VocabularyDetailModel : PageModel
{
    private readonly IMediator _mediator;

    public VocabularyDetailModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    [BindProperty(SupportsGet = true)]
    public int VocabularyId { get; set; }

    public VocabContextDto? Context { get; set; }

    public async Task<IActionResult> OnGetAsync(int vocabularyId)
    {
        VocabularyId = vocabularyId;
        Context = (await _mediator.Send(new GetVocabPassageQuery(vocabularyId))).Data;

        if (Context is null)
        {
            return NotFound();
        }

        return Page();
    }
}
