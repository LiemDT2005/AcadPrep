using System;
using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.Vocabulary.Commands.CreateVocabulary;

public class CreateVocabularyCommand : IRequest<Result<int>>
{
    public int UserId { get; set; }
    public string Word { get; set; } = string.Empty;
    public string? Phonetic { get; set; }
    public string Meaning { get; set; } = string.Empty;
    public string? Example { get; set; }
}
