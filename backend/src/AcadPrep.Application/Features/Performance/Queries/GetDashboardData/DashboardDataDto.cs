using System;
using System.Collections.Generic;

namespace AcadPrep.Application.Features.Performance.DTOs;

public class DashboardDataDto
{
    public List<ActiveExamDto> ActiveExams { get; set; } = new();
    public List<ActivityLogDto> RecentActivities { get; set; } = new();
    public SkillAnalyticsDto SkillAnalytics { get; set; } = new();
    public LeaderboardDto Leaderboard { get; set; } = new();
}

public class ActiveExamDto
{
    public int Id { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public string StatusText { get; set; } = string.Empty; // e.g. "35 min left · Part 5"
    public int ProgressPercentage { get; set; }
}

public class ActivityLogDto
{
    public string Description { get; set; } = string.Empty;
    public string TimeAgo { get; set; } = string.Empty;
    public string ColorType { get; set; } = "primary"; // primary, tertiary, error, etc.
    public DateTime CreatedAt { get; set; }
}

public class SkillAnalyticsDto
{
    // Part 1 to 7 percentages (0-100)
    public Dictionary<int, int> PartMastery { get; set; } = new();
}

public class LeaderboardDto
{
    public List<LeaderboardEntryDto> TopUsers { get; set; } = new();
    public LeaderboardEntryDto CurrentUser { get; set; } = new();
}

public class LeaderboardEntryDto
{
    public int Rank { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TotalScore { get; set; } // Could be exp or sum of scores
    public int ExamsDone { get; set; }
    public int StreakDays { get; set; }
    public bool IsCurrentUser { get; set; }
}
