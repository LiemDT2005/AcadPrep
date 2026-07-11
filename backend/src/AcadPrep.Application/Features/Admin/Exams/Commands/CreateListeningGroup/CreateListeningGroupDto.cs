using System.Collections.Generic;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.CreateListeningGroup;

public class CreateListeningGroupDto
{
    public int Part { get; set; }
    public required string Name { get; set; }
    public ListeningGroupMediaDto Media { get; set; } = new();
    public List<ListeningQuestionDto> Questions { get; set; } = new();
}

public class ListeningGroupMediaDto
{
    public string? AudioUrl { get; set; }
    public int? AudioStartSecond { get; set; }
    public int? AudioEndSecond { get; set; }
    public string? ImageUrl { get; set; }
    public bool UseExamFullAudio { get; set; }
}

public class ListeningQuestionDto
{
    public int QuestionNumber { get; set; }
    public string? QuestionText { get; set; }
    public string? ImageUrl { get; set; }
    public required string CorrectOption { get; set; }
    public List<ListeningOptionDto> Options { get; set; } = new();

    // Per-question audio (used for Part 1 & 2)
    public string? AudioUrl { get; set; }
    public bool UseExamFullAudio { get; set; }
    public int? AudioStartSecond { get; set; }
    public int? AudioEndSecond { get; set; }
}

public class ListeningOptionDto
{
    public required string Letter { get; set; }
    public required string Text { get; set; }
}
