using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using AcadPrep.Application.Features.Vocabulary.Queries.GetSavedVocabularies;
using AcadPrep.Application.Features.Vocabulary.Commands.RateVocabulary;
using AcadPrep.Application.Features.Vocabulary.Queries.GetReviewFlashcards;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcadPrep.WebUI.Pages.Vocabulary;

public class ReviewModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public ReviewModel(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    public List<SavedVocabularyDto> DueFlashcards { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        if (string.IsNullOrEmpty(_currentUserService.UserId) || !int.TryParse(_currentUserService.UserId, out int userId))
        {
            // For testing purposes, hardcode userId = 1 if not logged in
            userId = 2;
        }

        DueFlashcards = (await _mediator.Send(new GetReviewFlashcardsQuery(userId))).Data!;

        return Page();
    }

    public async Task<IActionResult> OnPostRateAsync(int vocabularyId, bool isRemembered)
    {
        if (string.IsNullOrEmpty(_currentUserService.UserId) || !int.TryParse(_currentUserService.UserId, out int userId))
        {
            userId = 2;
        }

        await _mediator.Send(new RateVocabularyCommand(userId, vocabularyId, isRemembered));

        return RedirectToPage();
    }
}


