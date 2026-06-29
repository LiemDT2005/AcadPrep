using Domain.Common;
using System;

namespace Domain.Entities;

public class PracticeSession : BaseEntity<int>
{
    public int UserId { get; set; }
    public int ExamId { get; set; }
    public string SelectedParts { get; set; } = null!; // JSON array or comma-separated list of part numbers
    public string? SelectedTags { get; set; } // JSON array or comma-separated list of tags
    public int? TimeLimit { get; set; } // in minutes
    public string CombinedQuestionsList { get; set; } = null!; // JSON array of question IDs
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Exam Exam { get; set; } = null!;
}
