using AcadPrep.Application.Common.Models;
using System.Linq;
using System.Threading;
using AcadPrep.Application.Features.Vocabulary.Queries.LookupVocabulary;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Vocabulary.Queries.LookupVocabulary;

public class LookupVocabularyQueryHandler : IRequestHandler<LookupVocabularyQuery, Result<VocabularyDetailDto?>>
{
    private readonly IAppDbContext _context;

    public LookupVocabularyQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<VocabularyDetailDto?>> Handle(LookupVocabularyQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Keyword))
        {
            return null;
        }

        var keyword = request.Keyword.Trim().ToLower();

        var vocabulary = await _context.Vocabularies
            .AsNoTracking()
            .Where(v => v.Word.ToLower() == keyword)
            .Select(v => new VocabularyDetailDto
            {
                VocabularyId = v.VocabularyId,
                Word = v.Word,
                Phonetic = v.Phonetic,
                Meaning = v.Meaning,
                Example = v.Example
            })
            .FirstOrDefaultAsync(cancellationToken);

        return vocabulary;
    }
}

