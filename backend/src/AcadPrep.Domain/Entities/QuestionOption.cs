namespace Domain.Entities;

public class QuestionOption
{
    public int OptionId { get; set; }
    public int QuestionId { get; set; }
    public string OptionLetter { get; set; } = null!;
    public string OptionText { get; set; } = null!;

    // Navigation properties
    public virtual Question Question { get; set; } = null!;
}
