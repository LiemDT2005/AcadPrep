using System.Threading.Tasks;
using Application.Common.Interfaces;
using AcadPrep.Application.Common.Models;
using AcadPrep.Application.Features.Vocabulary.Queries.GetSavedVocabularies;
using AcadPrep.Application.Features.Vocabulary.Commands.RemoveVocabulary;
using AcadPrep.Application.Features.Vocabulary.Commands.UpdateVocabulary;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcadPrep.WebUI.Pages.Vocabulary;

[Authorize]
public class NotebookModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public NotebookModel(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    public PaginatedList<SavedVocabularyDto> SavedVocabularies { get; set; } = default!;

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public async Task<IActionResult> OnGetAsync()
    {
        if (!int.TryParse(_currentUserService.UserId, out int userId))
        {
            return Unauthorized();
        }

        SavedVocabularies = (await _mediator.Send(new GetSavedVocabulariesQuery(userId, PageNumber, 10, SearchTerm))).Data!;

        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int vocabularyId)
    {
        if (!int.TryParse(_currentUserService.UserId, out int userId))
        {
            return Unauthorized();
        }

        var result = (await _mediator.Send(new RemoveVocabularyCommand(userId, vocabularyId))).Data!;
        
        if (!result)
        {
            ModelState.AddModelError("", "Vocabulary could not be found or removed.");
            // Reload page data to show error
            SavedVocabularies = (await _mediator.Send(new GetSavedVocabulariesQuery(userId, PageNumber, 10, SearchTerm))).Data!;
            return Page();
        }

        return RedirectToPage(new { PageNumber, SearchTerm });
    }

    [BindProperty]
    public AcadPrep.Application.Features.Vocabulary.Commands.CreateVocabulary.CreateVocabularyCommand CreateCommand { get; set; } = new();

    [BindProperty]
    public EditVocabularyInput EditInput { get; set; } = new();

    public async Task<IActionResult> OnPostEditAsync()
    {
        if (!int.TryParse(_currentUserService.UserId, out int userId))
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(EditInput.Word) || string.IsNullOrWhiteSpace(EditInput.Meaning))
        {
            ModelState.AddModelError("", "Word and Meaning are required.");
            SavedVocabularies = (await _mediator.Send(new GetSavedVocabulariesQuery(userId, PageNumber, 10, SearchTerm))).Data!;
            return Page();
        }

        var result = await _mediator.Send(new UpdateVocabularyCommand(
            userId,
            EditInput.VocabularyId,
            EditInput.Word,
            EditInput.Phonetic,
            EditInput.Meaning,
            EditInput.Example
        ));

        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.Error ?? "Failed to update vocabulary.");
            SavedVocabularies = (await _mediator.Send(new GetSavedVocabulariesQuery(userId, PageNumber, 10, SearchTerm))).Data!;
            return Page();
        }

        return RedirectToPage(new { PageNumber, SearchTerm });
    }

    public class EditVocabularyInput
    {
        public int VocabularyId { get; set; }
        public string Word { get; set; } = string.Empty;
        public string? Phonetic { get; set; }
        public string Meaning { get; set; } = string.Empty;
        public string? Example { get; set; }
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!int.TryParse(_currentUserService.UserId, out int userId))
        {
            return Unauthorized();
        }

        if (!ModelState.IsValid)
        {
            SavedVocabularies = (await _mediator.Send(new GetSavedVocabulariesQuery(userId, PageNumber, 10, SearchTerm))).Data!;
            return Page();
        }

        CreateCommand.UserId = userId;

        var result = await _mediator.Send(CreateCommand);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", "Failed to create vocabulary.");
            SavedVocabularies = (await _mediator.Send(new GetSavedVocabulariesQuery(userId, PageNumber, 10, SearchTerm))).Data!;
            return Page();
        }

        return RedirectToPage(new { PageNumber, SearchTerm });
    }
}
