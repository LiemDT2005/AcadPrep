using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using AcadPrep.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Vocabulary.Queries.GetSavedVocabularies;

public class GetSavedVocabulariesQueryHandler : IRequestHandler<GetSavedVocabulariesQuery, Result<PaginatedList<SavedVocabularyDto>>>
{
    private readonly IAppDbContext _context;

    public GetSavedVocabulariesQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedList<SavedVocabularyDto>>> Handle(GetSavedVocabulariesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.SavedVocabularies
            .Include(sv => sv.Vocabulary)
            .Where(sv => sv.UserId == request.UserId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(sv => sv.Vocabulary.Word.Contains(request.SearchTerm) || 
                                      sv.Vocabulary.Meaning.Contains(request.SearchTerm));
        }

        var dtoQuery = query
            .OrderByDescending(sv => sv.DateSaved)
            .Select(sv => new SavedVocabularyDto
            {
                VocabularyId = sv.VocabularyId,
                Word = sv.Vocabulary.Word,
                Phonetic = sv.Vocabulary.Phonetic,
                Meaning = sv.Vocabulary.Meaning,
                Example = sv.Vocabulary.Example,
                Interval = sv.Interval,
                NextReviewDate = sv.NextReviewDate,
                DateSaved = sv.DateSaved
            });

        var result = await PaginatedList<SavedVocabularyDto>.CreateAsync(dtoQuery, request.PageNumber, request.PageSize);
        return Result<PaginatedList<SavedVocabularyDto>>.Success(result);
    }
}
