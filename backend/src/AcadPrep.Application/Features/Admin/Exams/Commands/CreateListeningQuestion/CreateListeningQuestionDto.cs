using System.Collections.Generic;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.CreateListeningQuestion;

public class ListeningQuestionInputDto
{
    public int QuestionNumber { get; set; }
    public string? QuestionText { get; set; }
    public string? ImageUrl { get; set; }
    public string? Explanation { get; set; }
    public required string CorrectOption { get; set; }
    public List<ListeningQuestionOptionDto> Options { get; set; } = new();

    public string? AudioUrl { get; set; }
    public bool UseExamFullAudio { get; set; }
    public int? AudioStartSecond { get; set; }
    public int? AudioEndSecond { get; set; }
}

public class ListeningQuestionOptionDto
{
    public required string Letter { get; set; }
    public required string Text { get; set; }
}
