using System.Collections.Generic;
using System.Threading.Tasks;
using AcadPrep.Application.Features.Vocabulary.Queries.GetSavedVocabularies;
using AcadPrep.Application.Features.Vocabulary.Queries.GetVocabPassage;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcadPrep.WebUI.Pages.Vocabulary;

public class VocabularyDetailModel : PageModel
{
    private readonly IMediator _mediator;

    public VocabularyDetailModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    [BindProperty(SupportsGet = true)]
    public int VocabularyId { get; set; }

    public List<VocabPassageDto> Passages { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int vocabularyId)
    {
        VocabularyId = vocabularyId;
        Passages = (await _mediator.Send(new GetVocabPassageQuery(vocabularyId))).Data!;

        return Page();
    }
}


