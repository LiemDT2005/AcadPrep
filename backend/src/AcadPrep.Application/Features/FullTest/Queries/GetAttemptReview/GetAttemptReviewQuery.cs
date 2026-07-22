using System;
using System.Collections.Generic;
using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.FullTest.Queries.GetAttemptReview;

public record GetAttemptReviewQuery(int UserId, int? AttemptId = null, int? SessionId = null)
    : IRequest<Result<AttemptReviewDto>>;

public class AttemptReviewDto
{
    public bool IsPractice { get; set; }
    public int AttemptOrSessionId { get; set; }
    public int ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public int TotalScore { get; set; }
    public int MaxScore { get; set; }
    public int ListeningScore { get; set; }
    public int ReadingScore { get; set; }
    public int CorrectCount { get; set; }
    public int IncorrectCount { get; set; }
    public int UnansweredCount { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<ReviewQuestionDto> Questions { get; set; } = new();
}

public class ReviewQuestionDto
{
    public int QuestionId { get; set; }
    public int QuestionNumber { get; set; }
    public int Part { get; set; }
    public string? QuestionText { get; set; }
    public string? ImageUrl { get; set; }
    public string? AudioUrl { get; set; }
    public string? PassageContent { get; set; }
    public string? PassageImageUrl { get; set; }
    public string? TopicTag { get; set; }
    public string? SelectedOption { get; set; }
    public string CorrectOption { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public bool IsAnswered { get; set; }
    public string? Explanation { get; set; }
    public List<ReviewOptionDto> Options { get; set; } = new();
}

public class ReviewOptionDto
{
    public string Letter { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}
