using System;
using System.Collections.Generic;
using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.Performance.Queries.GetIncorrectAnswers;

public record GetIncorrectAnswersQuery(int UserId) : IRequest<Result<IncorrectAnswersResultDto>>;

public class IncorrectAnswersResultDto
{
    public int TotalIncorrect { get; set; }
    public List<IncorrectAnswerGroupDto> Groups { get; set; } = new();
}

public class IncorrectAnswerGroupDto
{
    public string GroupKey { get; set; } = string.Empty;
    public string GroupLabel { get; set; } = string.Empty;
    public List<IncorrectAnswerItemDto> Items { get; set; } = new();
}

public class IncorrectAnswerItemDto
{
    public int QuestionId { get; set; }
    public int QuestionNumber { get; set; }
    public int Part { get; set; }
    public int ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public int AttemptId { get; set; }
    public string? QuestionText { get; set; }
    public string? TopicTag { get; set; }
    public string? SelectedOption { get; set; }
    public string CorrectOption { get; set; } = string.Empty;
    public string? Explanation { get; set; }
    public DateTime? AttemptedAt { get; set; }
    public List<IncorrectOptionDto> Options { get; set; } = new();
}

public class IncorrectOptionDto
{
    public string Letter { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}
