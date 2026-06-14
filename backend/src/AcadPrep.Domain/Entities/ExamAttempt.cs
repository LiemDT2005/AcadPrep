using System;
using System.Collections.Generic;

namespace Domain.Entities;

public class ExamAttempt
{
    public int AttemptId { get; set; }
    public int UserId { get; set; }
    public int ExamId { get; set; }
    public int ListeningScore { get; set; } = 0;
    public int ReadingScore { get; set; } = 0;
    public int TotalScore { get; set; } = 0;
    public int RemainingTime { get; set; }
    public bool IsSubmitted { get; set; } = false;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Exam Exam { get; set; } = null!;
    public virtual ICollection<AttemptAnswer> AttemptAnswers { get; set; } = new List<AttemptAnswer>();
}
