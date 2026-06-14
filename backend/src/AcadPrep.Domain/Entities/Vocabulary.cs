using System;
using System.Collections.Generic;

namespace Domain.Entities;

public class Vocabulary
{
    public int VocabularyId { get; set; }
    public string Word { get; set; } = null!;
    public string? Phonetic { get; set; }
    public string Meaning { get; set; } = null!;
    public string? Example { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<VocabPassage> VocabPassages { get; set; } = new List<VocabPassage>();
    public virtual ICollection<SavedVocabulary> SavedVocabularies { get; set; } = new List<SavedVocabulary>();
}
