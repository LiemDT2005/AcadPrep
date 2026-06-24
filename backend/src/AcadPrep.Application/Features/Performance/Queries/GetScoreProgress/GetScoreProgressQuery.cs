using AcadPrep.Application.Common.Models;
using System;
using System.Collections.Generic;
using MediatR;

namespace AcadPrep.Application.Features.Performance.Queries.GetScoreProgress;

public record GetScoreProgressQuery(int UserId) : IRequest<Result<ScoreProgressResultDto>>;

public class ScoreProgressResultDto
{
    public bool HasSufficientData { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<ScoreProgressDto> Scores { get; set; } = new();
}

public class ScoreProgressDto
{
    public DateTime AttemptDate { get; set; }
    public int Score { get; set; }
}

