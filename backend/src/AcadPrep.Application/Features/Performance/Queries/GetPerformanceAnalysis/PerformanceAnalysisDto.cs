using System.Collections.Generic;

namespace AcadPrep.Application.Features.Performance.Queries.GetPerformanceAnalysis;

public class PerformanceAnalysisDto
{
    public bool HasData { get; set; }
    public string Message { get; set; } = string.Empty;
    public double AverageListeningScore { get; set; }
    public double AverageReadingScore { get; set; }
    public int TotalTestsTaken { get; set; }
    public Dictionary<int, int> PartMastery { get; set; } = new();
}
