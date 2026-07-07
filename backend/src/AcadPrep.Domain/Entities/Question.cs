using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class Question : BaseEntity<int>
{
    public int QuestionNumber { get; set; }
    public int Part { get; set; }
    public string? QuestionText { get; set; }
    
    public string? AudioUrl { get; set; }
    public int? AudioStartSecond { get; set; } // Giây bắt đầu trên file Exam.AudioUrl (nếu dùng)
    public int? AudioEndSecond { get; set; } // Giây kết thúc trên file Exam.AudioUrl (nếu dùng)
    
    public string? ImageUrl { get; set; } 
    
    public OptionLetter CorrectOption { get; set; }
    public int ExamId { get; set; }
    public int? PassageId { get; set; }
    public string? QuestionType { get; set; }
    public string? TopicTag { get; set; }
    public int? QuestionGroupId { get; set; }

    // Navigation properties
    public virtual Exam Exam { get; set; } = null!;
    public virtual Passage? Passage { get; set; }
    public virtual QuestionGroup? QuestionGroup { get; set; }
    public virtual ICollection<QuestionOption> QuestionOptions { get; set; } = new List<QuestionOption>();
    public virtual ICollection<AttemptAnswer> AttemptAnswers { get; set; } = new List<AttemptAnswer>();
}
