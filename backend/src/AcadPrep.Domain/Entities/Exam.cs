using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class Exam : BaseEntity<int>, IAuditable, ISoftDeletable
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int Duration { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastModifiedAt { get; set; }
    
    public string? AudioUrl { get; set; } // File > 45 phút
    public ExamStatus Status { get; set; } = ExamStatus.Draft; // Draft, Published, Archived

    public int ExamSeriesId { get; set; }
    public virtual ExamSeries ExamSeries { get; set; } = null!;

    // Navigation properties
    public virtual ICollection<Passage> Passages { get; set; } = new List<Passage>();
    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
    public virtual ICollection<QuestionGroup> QuestionGroups { get; set; } = new List<QuestionGroup>();
    public virtual ICollection<ExamAttempt> ExamAttempts { get; set; } = new List<ExamAttempt>();

    public void SoftDelete() => IsDeleted = true;
}

