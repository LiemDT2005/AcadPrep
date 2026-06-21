namespace AcadPrep.Application.Features.Admin.DTOs;

public class ExamStatsDto
{
    public int ExamId { get; set; }
    public string ExamName { get; set; } = string.Empty;
    public int TotalAttempts { get; set; }
    public double AverageScore { get; set; }
    public decimal HighestScore { get; set; }
    public decimal LowestScore { get; set; }
}
