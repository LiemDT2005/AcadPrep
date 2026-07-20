using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.Vocabulary.Commands.UpdateVocabulary;

public record UpdateVocabularyCommand(
    int UserId,
    int VocabularyId,
    string Word,
    string? Phonetic,
    string Meaning,
    string? Example
) : IRequest<Result<bool>>;
