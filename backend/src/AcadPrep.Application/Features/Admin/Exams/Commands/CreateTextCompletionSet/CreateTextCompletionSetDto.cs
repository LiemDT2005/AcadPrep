using System.Collections.Generic;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.CreateTextCompletionSet;

public class CreateTextCompletionSetDto
{
    public required TextCompletionPassageDto Passage { get; set; }
    public List<TextCompletionQuestionDto> Questions { get; set; } = new();
}

public class TextCompletionPassageDto
{
    public string? Content { get; set; }
    public string? ImageUrl { get; set; }
    public string? Explanation { get; set; }
}

public class TextCompletionQuestionDto
{
    public int QuestionNumber { get; set; }
    public string? QuestionText { get; set; }
    public required string CorrectOption { get; set; }
    public List<TextCompletionOptionDto> Options { get; set; } = new();
}

public class TextCompletionOptionDto
{
    public required string Letter { get; set; }
    public required string Text { get; set; }
}
