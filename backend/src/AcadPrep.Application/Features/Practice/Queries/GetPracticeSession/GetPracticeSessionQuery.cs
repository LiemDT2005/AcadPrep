using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.Practice.Queries.GetPracticeSession;

public record GetPracticeSessionQuery(int SessionId, int UserId) : IRequest<Result<PracticeSessionDto>>;

public class PracticeSessionDto
{
    public int SessionId { get; set; }
    public int ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public string? ExamAudioUrl { get; set; }
    public int? TimeLimitMinutes { get; set; }
    public List<int> SelectedParts { get; set; } = new();
    public List<PracticeQuestionDto> Questions { get; set; } = new();
}

public class PracticeQuestionDto
{
    public int Id { get; set; }
    public int QuestionNumber { get; set; }
    public int Part { get; set; }
    public string? QuestionText { get; set; }
    public string? AudioUrl { get; set; }
    public int? AudioStartSecond { get; set; }
    public int? AudioEndSecond { get; set; }
    public string? ImageUrl { get; set; }
    public List<PracticeQuestionOptionDto> Options { get; set; } = new();
}

public class PracticeQuestionOptionDto
{
    public string Letter { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}
