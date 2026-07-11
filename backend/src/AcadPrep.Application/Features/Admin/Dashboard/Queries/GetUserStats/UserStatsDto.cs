namespace AcadPrep.Application.Features.Admin.Dashboard.Queries.GetUserStats;

public class UserStatsDto
{
    public int TotalUsers { get; set; }
    public int NewRegistrations { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalExamsTaken { get; set; }
    public double AverageToeicScore { get; set; }
}
