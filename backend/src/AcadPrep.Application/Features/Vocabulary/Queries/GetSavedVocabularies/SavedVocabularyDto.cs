using System;

namespace AcadPrep.Application.Features.Vocabulary.Queries.GetSavedVocabularies;

public class SavedVocabularyDto
{
    public int VocabularyId { get; set; }
    public string Word { get; set; } = string.Empty;
    public string Phonetic { get; set; } = string.Empty;
    public string Meaning { get; set; } = string.Empty;
    public string? Example { get; set; }
    public int Interval { get; set; }
    public DateTime NextReviewDate { get; set; }
    public DateTime DateSaved { get; set; }
}
