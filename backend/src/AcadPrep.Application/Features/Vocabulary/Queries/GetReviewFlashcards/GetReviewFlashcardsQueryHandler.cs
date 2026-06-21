using AcadPrep.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using AcadPrep.Application.Features.Vocabulary.Queries.GetSavedVocabularies;

namespace AcadPrep.Application.Features.Vocabulary.Queries.GetReviewFlashcards;

public class GetReviewFlashcardsQueryHandler : IRequestHandler<GetReviewFlashcardsQuery, Result<List<SavedVocabularyDto>>>
{
    private readonly IAppDbContext _context;

    public GetReviewFlashcardsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<SavedVocabularyDto>>> Handle(GetReviewFlashcardsQuery request, CancellationToken cancellationToken)
    {
        var dueFlashcards = await _context.SavedVocabularies
            .Include(sv => sv.Vocabulary)
            .Where(sv => sv.UserId == request.UserId && sv.NextReviewDate <= DateTime.UtcNow)
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
            })
            .ToListAsync(cancellationToken);

        return dueFlashcards;
    }
}
