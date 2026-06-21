namespace Domain.Entities;

public class StudyStreak
{
    public int UserId { get; set; }
    public int CurrentStreak { get; set; }
    public int MaxStreak { get; set; }
    public DateOnly LastActiveDate { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
}
