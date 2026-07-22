using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.FullTest.Queries.GetTestSession;

public record GetTestSessionQuery(int AttemptId, int UserId) : IRequest<Result<TestSessionDto>>;

public class TestSessionDto
{
    public int AttemptId { get; set; }
    public int ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public string? ExamAudioUrl { get; set; }
    public int RemainingSeconds { get; set; }
    public bool IsSubmitted { get; set; }
    public int CurrentQuestionIndex { get; set; }
    public List<TestQuestionDto> Questions { get; set; } = new();
    public Dictionary<int, string> SavedAnswers { get; set; } = new();
}

public class TestQuestionDto
{
    public int Id { get; set; }
    public int QuestionNumber { get; set; }
    public int Part { get; set; }
    public string? QuestionText { get; set; }
    public string? AudioUrl { get; set; }
    public int? AudioStartSecond { get; set; }
    public int? AudioEndSecond { get; set; }
    public string? ImageUrl { get; set; }
    public int? PassageId { get; set; }
    public int? QuestionGroupId { get; set; }
    public string? GroupAudioUrl { get; set; }
    public int? GroupAudioStartSecond { get; set; }
    public int? GroupAudioEndSecond { get; set; }
    public string? GroupImageUrl { get; set; }
    /// <summary>Shared reading stimulus (Part 6 passage, Part 7 set passages).</summary>
    public List<SessionPassageDto> Passages { get; set; } = new();
    public List<TestQuestionOptionDto> Options { get; set; } = new();
}

public class TestQuestionOptionDto
{
    public string Letter { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}
