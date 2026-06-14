using System;

namespace Domain.Entities;

public class StudyStreak
{
    public int UserId { get; set; }
    public int CurrentStreak { get; set; } = 0;
    public int MaxStreak { get; set; } = 0;
    public DateOnly LastActiveDate { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
}
