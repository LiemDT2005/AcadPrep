using Domain.Common;
using System.Collections.Generic;

namespace Domain.Entities;

public class QuestionGroup : BaseEntity<int>
{
    public string Name { get; set; } = null!;

    // Hỗ trợ Audio lẻ cho nhóm câu và mốc thời gian trên file full
    public string? AudioUrl { get; set; } // File audio lẻ của đoạn hội thoại
    public int? AudioStartSecond { get; set; } // Giây bắt đầu trên file Exam.AudioUrl
    public int? AudioEndSecond { get; set; } // Giây kết thúc trên file Exam.AudioUrl
    
    public string? ImageUrl { get; set; } 
    
    public int ExamId { get; set; }
    
    // Navigation properties
    public virtual Exam Exam { get; set; } = null!;
    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
