using Domain.Common;

namespace Domain.Entities;

public class VocabPassage : BaseEntity<int>
{
    public string Content { get; set; } = null!;
    public int VocabularyId { get; set; }

    // Navigation properties
    public virtual Vocabulary Vocabulary { get; set; } = null!;
}
