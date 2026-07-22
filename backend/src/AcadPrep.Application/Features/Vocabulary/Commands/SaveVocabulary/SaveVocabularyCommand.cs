using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.Vocabulary.Commands.SaveVocabulary;

public record SaveVocabularyCommand(int UserId, int VocabularyId) : IRequest<Result<bool>>;

