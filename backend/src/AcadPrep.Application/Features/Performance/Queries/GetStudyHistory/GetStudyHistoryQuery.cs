using System;
using System.Collections.Generic;
using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.Performance.Queries.GetStudyHistory;

public record GetStudyHistoryQuery(int UserId) : IRequest<Result<StudyHistoryResultDto>>;

public class StudyHistoryResultDto
{
    public List<StudyHistoryItemDto> Items { get; set; } = new();
}

public class StudyHistoryItemDto
{
    public string ActivityType { get; set; } = string.Empty; // exam | practice | vocab
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public string? LinkUrl { get; set; }
    public string Icon { get; set; } = "history";
    public string ColorType { get; set; } = "primary";
}
