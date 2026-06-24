using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.Vocabulary.Queries.GetSavedVocabularies;

public record GetSavedVocabulariesQuery(int UserId, int PageNumber = 1, int PageSize = 10, string? SearchTerm = null) : IRequest<Result<PaginatedList<SavedVocabularyDto>>>;
