using System;
using System.Collections.Generic;

namespace AcadPrep.Application.Features.Admin.DTOs;

public class ComprehensiveReportDto
{
    public List<ProgressDataPoint> DataPoints { get; set; } = new();
}

public class ProgressDataPoint
{
    public DateOnly Date { get; set; }
    public int CompletedExamsCount { get; set; }
}
