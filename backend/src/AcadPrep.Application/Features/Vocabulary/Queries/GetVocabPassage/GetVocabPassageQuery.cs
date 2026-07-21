using AcadPrep.Application.Common.Models;
using AcadPrep.Application.Features.Vocabulary.Queries.GetVocabPassage;
using MediatR;

namespace AcadPrep.Application.Features.Vocabulary.Queries.GetVocabPassage;

public record GetVocabPassageQuery(int VocabularyId) : IRequest<Result<VocabContextDto?>>;

