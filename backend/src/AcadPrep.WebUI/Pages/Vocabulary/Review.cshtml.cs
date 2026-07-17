using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AcadPrep.Application.Common.Models;
using Application.Common.Interfaces;
using AcadPrep.Application.Features.Vocabulary.Queries.GetSavedVocabularies;
using AcadPrep.Application.Features.Vocabulary.Commands.RateVocabulary;
using AcadPrep.Application.Features.Vocabulary.Queries.GetReviewFlashcards;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcadPrep.WebUI.Pages.Vocabulary;

[Authorize]
public class ReviewModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public ReviewModel(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    public PaginatedList<SavedVocabularyDto>? DueFlashcards { get; set; }

    public async Task<IActionResult> OnGetAsync(int pageNumber = 1)
    {
        if (!int.TryParse(_currentUserService.UserId, out int userId))
        {
            return Unauthorized();
        }

        DueFlashcards = (await _mediator.Send(new GetReviewFlashcardsQuery(userId, pageNumber, 10))).Data;

        return Page();
    }

    public async Task<IActionResult> OnPostRateAsync(int vocabularyId, bool isRemembered)
    {
        if (!int.TryParse(_currentUserService.UserId, out int userId))
        {
            return Unauthorized();
        }

        await _mediator.Send(new RateVocabularyCommand(userId, vocabularyId, isRemembered));

        return RedirectToPage();
    }
}
