namespace AcadPrep.Application.Features.Vocabulary.Queries.LookupVocabulary;

public class VocabularyDetailDto
{
    public int VocabularyId { get; set; }
    public string Word { get; set; } = string.Empty;
    public string Phonetic { get; set; } = string.Empty;
    public string Meaning { get; set; } = string.Empty;
    public string? Example { get; set; }
}
