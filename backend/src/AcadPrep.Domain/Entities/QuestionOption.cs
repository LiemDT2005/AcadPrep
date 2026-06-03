using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class QuestionOption : BaseEntity<int>
{
    public int QuestionId { get; set; }
    public OptionLetter OptionLetter { get; set; }
    public string OptionText { get; set; } = null!;

    // Navigation properties
    public virtual Question Question { get; set; } = null!;
}
