using AcadPrep.Application.Common.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Vocabulary.Commands.SaveVocabulary;

public class SaveVocabularyCommandHandler : IRequestHandler<SaveVocabularyCommand, Result<bool>>
{
    private readonly IAppDbContext _context;
    private readonly IMediator _mediator;

    public SaveVocabularyCommandHandler(IAppDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<Result<bool>> Handle(SaveVocabularyCommand request, CancellationToken cancellationToken)
    {
        var exists = await _context.SavedVocabularies
            .AnyAsync(sv => sv.UserId == request.UserId && sv.VocabularyId == request.VocabularyId, cancellationToken);

        if (exists)
        {
            return false; // Or throw an exception for duplicate
        }

        var entity = new SavedVocabulary
        {
            UserId = request.UserId,
            VocabularyId = request.VocabularyId,
            Interval = 1,
            NextReviewDate = DateTime.UtcNow.Date.AddDays(1),
            DateSaved = DateTime.UtcNow
        };

        _context.SavedVocabularies.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        // Check for vocabulary-related achievements
        await _mediator.Send(new AcadPrep.Application.Features.Performance.Commands.CheckAndGrantAchievements.CheckAndGrantAchievementsCommand(request.UserId), cancellationToken);

        return true;
    }
}
