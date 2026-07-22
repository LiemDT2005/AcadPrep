using AcadPrep.Application.Common.Models;
using AcadPrep.Application.Features.Vocabulary.Queries.LookupVocabulary;
using MediatR;

namespace AcadPrep.Application.Features.Vocabulary.Queries.LookupVocabulary;

public record LookupVocabularyQuery(string Keyword) : IRequest<Result<VocabularyDetailDto?>>;

