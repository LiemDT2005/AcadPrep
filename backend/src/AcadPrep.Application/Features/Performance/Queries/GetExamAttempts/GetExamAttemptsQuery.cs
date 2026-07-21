using System;
using System.Collections.Generic;
using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.Performance.Queries.GetExamAttempts;

public record GetExamAttemptsQuery(int UserId) : IRequest<Result<ExamAttemptsResultDto>>;

public class ExamAttemptsResultDto
{
    public List<ExamAttemptListItemDto> Attempts { get; set; } = new();
}

public class ExamAttemptListItemDto
{
    public int AttemptId { get; set; }
    public int ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public int ListeningScore { get; set; }
    public int ReadingScore { get; set; }
    public int TotalScore { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
