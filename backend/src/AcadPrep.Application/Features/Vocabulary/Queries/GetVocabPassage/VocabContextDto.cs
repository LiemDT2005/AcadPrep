namespace AcadPrep.Application.Features.Vocabulary.Queries.GetVocabPassage;

public class VocabContextDto
{
    public int VocabularyId { get; set; }
    public string Word { get; set; } = string.Empty;
    public string? Phonetic { get; set; }
    public string Meaning { get; set; } = string.Empty;
    public string? Example { get; set; }
    public List<VocabPassageDto> Passages { get; set; } = new();
}
