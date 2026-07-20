using System;
using System.Threading;
using System.Threading.Tasks;
using AcadPrep.Application.Common.Models;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Vocabulary.Commands.UpdateVocabulary;

public class UpdateVocabularyCommandHandler : IRequestHandler<UpdateVocabularyCommand, Result<bool>>
{
    private readonly IAppDbContext _context;

    public UpdateVocabularyCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(UpdateVocabularyCommand request, CancellationToken cancellationToken)
    {
        var savedVocab = await _context.SavedVocabularies
            .Include(sv => sv.Vocabulary)
            .FirstOrDefaultAsync(sv => sv.UserId == request.UserId && sv.VocabularyId == request.VocabularyId, cancellationToken);

        if (savedVocab is null)
        {
            return Result<bool>.Failure("Vocabulary not found in your notebook.");
        }

        var vocab = savedVocab.Vocabulary;
        vocab.Word = request.Word.Trim();
        vocab.Phonetic = request.Phonetic?.Trim();
        vocab.Meaning = request.Meaning.Trim();
        vocab.Example = request.Example?.Trim();
        vocab.LastModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
