using Domain.Common;

namespace Domain.Entities;

public class Passage : BaseEntity<int>
{
    public string? Content { get; set; } 
    public string? ImageUrl { get; set; } 
    
    public int ExamId { get; set; }

    // Navigation properties
    public virtual Exam Exam { get; set; } = null!;
    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
