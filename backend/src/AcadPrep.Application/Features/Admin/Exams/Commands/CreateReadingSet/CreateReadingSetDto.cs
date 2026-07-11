using System.Collections.Generic;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.CreateReadingSet;

public class CreateReadingSetDto
{
    public required string Name { get; set; }
    public List<ReadingPassageDto> Passages { get; set; } = new();
    public List<ReadingQuestionDto> Questions { get; set; } = new();
}

public class ReadingPassageDto
{
    public string? Content { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
}

public class ReadingQuestionDto
{
    public int QuestionNumber { get; set; }
    public required string QuestionText { get; set; }
    public required string CorrectOption { get; set; }
    public List<ReadingOptionDto> Options { get; set; } = new();
}

public class ReadingOptionDto
{
    public required string Letter { get; set; }
    public required string Text { get; set; }
}
