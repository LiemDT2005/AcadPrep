using AcadPrep.Application.Common.Models;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Vocabulary.Commands.RemoveVocabulary;

public class RemoveVocabularyCommandHandler : IRequestHandler<RemoveVocabularyCommand, Result<bool>>
{
    private readonly IAppDbContext _context;

    public RemoveVocabularyCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(RemoveVocabularyCommand request, CancellationToken cancellationToken)
    {
        var savedVocab = await _context.SavedVocabularies
            .FirstOrDefaultAsync(sv => sv.UserId == request.UserId && sv.VocabularyId == request.VocabularyId, cancellationToken);

        if (savedVocab == null)
        {
            return false;
        }

        _context.SavedVocabularies.Remove(savedVocab);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
