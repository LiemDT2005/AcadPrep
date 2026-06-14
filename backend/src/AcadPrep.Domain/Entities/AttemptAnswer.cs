namespace Domain.Entities;

public class AttemptAnswer
{
    public int AttemptId { get; set; }
    public int QuestionId { get; set; }
    public string? SelectedOption { get; set; }
    public bool IsCorrect { get; set; } = false;

    // Navigation properties
    public virtual ExamAttempt ExamAttempt { get; set; } = null!;
    public virtual Question Question { get; set; } = null!;
}
