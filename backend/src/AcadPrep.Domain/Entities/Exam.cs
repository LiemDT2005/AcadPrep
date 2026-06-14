using System;
using System.Collections.Generic;

namespace Domain.Entities;

public class Exam
{
    public int ExamId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int Duration { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<Passage> Passages { get; set; } = new List<Passage>();
    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
    public virtual ICollection<ExamAttempt> ExamAttempts { get; set; } = new List<ExamAttempt>();
}
