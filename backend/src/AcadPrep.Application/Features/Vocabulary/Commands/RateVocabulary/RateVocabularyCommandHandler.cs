using AcadPrep.Application.Common.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Vocabulary.Commands.RateVocabulary;

public class RateVocabularyCommandHandler : IRequestHandler<RateVocabularyCommand, Result<bool>>
{
    private readonly IAppDbContext _context;

    public RateVocabularyCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(RateVocabularyCommand request, CancellationToken cancellationToken)
    {
        var savedVocab = await _context.SavedVocabularies
            .FirstOrDefaultAsync(sv => sv.UserId == request.UserId && sv.VocabularyId == request.VocabularyId, cancellationToken);

        if (savedVocab == null)
        {
            return false; // Or throw NotFoundException
        }

        if (request.IsRemembered)
        {
            savedVocab.Interval = savedVocab.Interval * 2;
        }
        else
        {
            savedVocab.Interval = 1;
        }

        savedVocab.NextReviewDate = DateTime.UtcNow.Date.AddDays(savedVocab.Interval);
        
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
