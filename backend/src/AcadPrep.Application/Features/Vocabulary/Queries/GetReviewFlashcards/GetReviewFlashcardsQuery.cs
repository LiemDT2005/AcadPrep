using AcadPrep.Application.Common.Models;
using System.Collections.Generic;
using MediatR;
using AcadPrep.Application.Features.Vocabulary.Queries.GetSavedVocabularies;

namespace AcadPrep.Application.Features.Vocabulary.Queries.GetReviewFlashcards;

public record GetReviewFlashcardsQuery(int UserId, int PageNumber = 1, int PageSize = 50) : IRequest<Result<PaginatedList<SavedVocabularyDto>>>;

