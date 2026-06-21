using AcadPrep.Application.Common.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AcadPrep.Application.Features.Vocabulary.Queries.GetVocabPassage;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Vocabulary.Queries.GetVocabPassage;

public class GetVocabPassageQueryHandler : IRequestHandler<GetVocabPassageQuery, Result<List<VocabPassageDto>>>
{
    private readonly IAppDbContext _context;

    public GetVocabPassageQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<VocabPassageDto>>> Handle(GetVocabPassageQuery request, CancellationToken cancellationToken)
    {
        var passages = await _context.VocabPassages
            .AsNoTracking()
            .Where(p => p.VocabularyId == request.VocabularyId)
            .Select(p => new VocabPassageDto
            {
                VocabPassageId = p.VocabPassageId,
                VocabularyId = p.VocabularyId,
                Content = p.Content
            })
            .ToListAsync(cancellationToken);

        return passages;
    }
}

