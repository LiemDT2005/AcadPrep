using Domain.Common;

namespace Domain.Entities;

public class Part : BaseEntity<int>
{
    public int ExamId { get; set; }
    public int PartNumber { get; set; }
    public int TotalQuestions { get; set; }

    // Navigation properties
    public virtual Exam Exam { get; set; } = null!;
}
