using System.Collections.Generic;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.CreatePart5Question;

public class Part5QuestionDto
{
    public int QuestionNumber { get; set; }
    public string? QuestionText { get; set; }
    public required string CorrectOption { get; set; }
    public string? QuestionType { get; set; }
    public string? TopicTag { get; set; }
    public string? Explanation { get; set; }
    public List<Part5OptionDto> Options { get; set; } = new();
}

public class Part5OptionDto
{
    public required string Letter { get; set; }
    public required string Text { get; set; }
}
