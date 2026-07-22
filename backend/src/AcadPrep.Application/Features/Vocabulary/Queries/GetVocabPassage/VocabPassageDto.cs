namespace AcadPrep.Application.Features.Vocabulary.Queries.GetVocabPassage;

public class VocabPassageDto
{
    public int VocabPassageId { get; set; }
    public int VocabularyId { get; set; }
    public string Content { get; set; } = string.Empty;
}
