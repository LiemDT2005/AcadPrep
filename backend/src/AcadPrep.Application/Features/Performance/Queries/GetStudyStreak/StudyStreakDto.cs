using System;

namespace AcadPrep.Application.Features.Performance.Queries.GetStudyStreak;

public class StudyStreakDto
{
    public int CurrentStreak { get; set; }
    public int MaxStreak { get; set; }
    public DateOnly LastActiveDate { get; set; }
}
