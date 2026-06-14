using System;

namespace Domain.Entities;

public class SavedVocabulary
{
    public int UserId { get; set; }
    public int VocabularyId { get; set; }
    public int Interval { get; set; } = 1;
    public DateTime DateSaved { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Vocabulary Vocabulary { get; set; } = null!;
}
