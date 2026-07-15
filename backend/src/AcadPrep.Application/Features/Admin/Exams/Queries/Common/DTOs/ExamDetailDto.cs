using System;
using System.Collections.Generic;

namespace AcadPrep.Application.Features.Admin.Exams.Queries.Common.DTOs;

public class ExamDetailDto
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public int Duration { get; set; }
    public int ExamSeriesId { get; set; }
    public string? ExamSeriesName { get; set; }
    public string? AudioUrl { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public int AttemptCount { get; set; }
    public List<QuestionDetailDto> Questions { get; set; } = new();
    public List<ReadingSetDto> Part7ReadingSets { get; set; } = new();
}

public class QuestionDetailDto
{
    public int Id { get; set; }
    public int QuestionNumber { get; set; }
    public int Part { get; set; }
    public string? QuestionText { get; set; }
    public string? AudioUrl { get; set; }
    public string? ImageUrl { get; set; }
    public required string CorrectOption { get; set; }
    public int? PassageId { get; set; }
    public int? PassageDisplayOrder { get; set; }
    public string? PassageContent { get; set; }
    public string? PassageImageUrl { get; set; }
    public int? QuestionGroupId { get; set; }
    public string? QuestionType { get; set; }
    public string? TopicTag { get; set; }
    public List<QuestionOptionDto> Options { get; set; } = new();
}

public class QuestionOptionDto
{
    public required string OptionLetter { get; set; }
    public required string OptionText { get; set; }
}
