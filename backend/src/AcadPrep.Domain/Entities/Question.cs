using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class Question : BaseEntity<int>
{
    public int QuestionNumber { get; set; }
    public int Part { get; set; }
    public string? QuestionText { get; set; }
    public string? AudioUrl { get; set; }
    public OptionLetter CorrectOption { get; set; }
    public int ExamId { get; set; }
    public int? PassageId { get; set; }

    // Navigation properties
    public virtual Exam Exam { get; set; } = null!;
    public virtual Passage? Passage { get; set; }
    public virtual ICollection<QuestionOption> QuestionOptions { get; set; } = new List<QuestionOption>();
    public virtual ICollection<AttemptAnswer> AttemptAnswers { get; set; } = new List<AttemptAnswer>();
}
