using AcadPrep.Application.Common.Models;
using System.Collections.Generic;
using MediatR;
using AcadPrep.Application.Features.Vocabulary.Queries.GetSavedVocabularies;

namespace AcadPrep.Application.Features.Vocabulary.Queries.GetReviewFlashcards;

public record GetReviewFlashcardsQuery(int UserId) : IRequest<Result<List<SavedVocabularyDto>>>;

