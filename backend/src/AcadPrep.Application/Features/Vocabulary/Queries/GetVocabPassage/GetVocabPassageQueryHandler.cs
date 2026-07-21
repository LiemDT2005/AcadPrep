using AcadPrep.Application.Common.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AcadPrep.Application.Features.Vocabulary.Queries.GetVocabPassage;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Vocabulary.Queries.GetVocabPassage;

public class GetVocabPassageQueryHandler : IRequestHandler<GetVocabPassageQuery, Result<VocabContextDto?>>
{
    private readonly IAppDbContext _context;

    public GetVocabPassageQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<VocabContextDto?>> Handle(GetVocabPassageQuery request, CancellationToken cancellationToken)
    {
        var vocabulary = await _context.Vocabularies
            .AsNoTracking()
            .Where(v => v.Id == request.VocabularyId)
            .Select(v => new VocabContextDto
            {
                VocabularyId = v.Id,
                Word = v.Word,
                Phonetic = v.Phonetic,
                Meaning = v.Meaning,
                Example = v.Example
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (vocabulary is null)
        {
            return null;
        }

        vocabulary.Passages = await _context.VocabPassages
            .AsNoTracking()
            .Where(p => p.VocabularyId == request.VocabularyId)
            .Select(p => new VocabPassageDto
            {
                VocabPassageId = p.Id,
                VocabularyId = p.VocabularyId,
                Content = p.Content
            })
            .ToListAsync(cancellationToken);

        return vocabulary;
    }
}

