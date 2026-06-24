using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.Vocabulary.Commands.RateVocabulary;

public record RateVocabularyCommand(int UserId, int VocabularyId, bool IsRemembered) : IRequest<Result<bool>>;

