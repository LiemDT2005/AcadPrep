using Domain.Common;

namespace Domain.Entities;

public class Vocabulary : BaseEntity<int>, IAuditable
{
    public string Word { get; set; } = null!;
    public string? Phonetic { get; set; }
    public string Meaning { get; set; } = null!;
    public string? Example { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastModifiedAt { get; set; }

    // Navigation properties
    public virtual ICollection<VocabPassage> VocabPassages { get; set; } = new List<VocabPassage>();
    public virtual ICollection<SavedVocabulary> SavedVocabularies { get; set; } = new List<SavedVocabulary>();
}
