using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.Vocabulary.Commands.RemoveVocabulary;

public record RemoveVocabularyCommand(int UserId, int VocabularyId) : IRequest<Result<bool>>;

