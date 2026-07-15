using System.Collections.Generic;
using AcadPrep.Application.Features.Admin.Exams.Commands.CreateTextCompletionSet;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.UpdateTextCompletionSet;

public class UpdateTextCompletionSetDto
{
    public required TextCompletionPassageDto Passage { get; set; }
    public List<UpdateTextCompletionQuestionDto> Questions { get; set; } = new();
}

public class UpdateTextCompletionQuestionDto
{
    public int Id { get; set; }
    public int QuestionNumber { get; set; }
    public string? QuestionText { get; set; }
    public required string CorrectOption { get; set; }
    public List<TextCompletionOptionDto> Options { get; set; } = new();
}
